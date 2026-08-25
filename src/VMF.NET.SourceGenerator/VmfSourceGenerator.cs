// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VMF.NET.Core;

using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.SourceGenerator;

/// <summary>
/// Roslyn incremental source generator for VMF.NET.
/// Discovers interfaces annotated with VMF attributes, runs model analysis,
/// and emits implementation classes via Scriban templates.
/// </summary>
[Generator]
public sealed class VmfSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Find the interfaces that live in a model namespace
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (ctx, _) => GetVmfInterfaceSymbol(ctx))
            .Where(static symbol => symbol != null)
            .Select(static (symbol, _) => symbol!);

        // Step 2: Combine all interfaces and the compilation
        var collected = interfaceDeclarations.Collect();

        // Step 3: Generate source
        context.RegisterSourceOutput(collected, static (spc, interfaces) => Execute(spc, interfaces));
    }

    /// <summary>
    /// Semantic transform: a model type is one declared in a model namespace. Nothing else marks
    /// it -- mirroring Java, where an interface is a model because it sits in the
    /// <c>vmfmodel</c> package.
    /// </summary>
    private static INamedTypeSymbol? GetVmfInterfaceSymbol(GeneratorSyntaxContext context)
    {
        var interfaceSyntax = (InterfaceDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(interfaceSyntax) is not INamedTypeSymbol symbol)
            return null;

        return ModelNaming.IsModelType(symbol) ? symbol : null;
    }

    /// <summary>
    /// Main generation logic.
    /// </summary>
    private static void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> interfaces)
    {
        if (interfaces.IsDefaultOrEmpty) return;

        // Group by the namespace the API is generated INTO -- the model namespace's parent -- so
        // one `MyApp.VmfModel` forms the model for `MyApp`.
        var byNamespace = new Dictionary<string, List<INamedTypeSymbol>>();
        foreach (var iface in interfaces)
        {
            var ns = ModelNaming.ApiNamespace(iface);
            if (!byNamespace.TryGetValue(ns, out var list))
            {
                list = new List<INamedTypeSymbol>();
                byNamespace[ns] = list;
            }
            // Deduplicate (same interface may appear from multiple partial declarations)
            if (!list.Any(s => SymbolEqualityComparer.Default.Equals(s, iface)))
                list.Add(iface);
        }

        var renderer = new TemplateRenderer();

        foreach (var kvp in byNamespace)
        {
            var ns = kvp.Key;
            var symbols = kvp.Value;

            // Extract symbol data
            var symbolDataList = new List<TypeSymbolData>();
            foreach (var symbol in symbols)
            {
                symbolDataList.Add(SymbolExtractor.Extract(symbol));
            }

            // Run model analysis
            var model = ModelAnalyzer.Analyze(ns, symbolDataList);

            // Report diagnostics
            foreach (var diag in model.Diagnostics)
            {
                var severity = diag.Severity == Core.DiagnosticSeverity.Error
                    ? DiagnosticSeverity.Error
                    : DiagnosticSeverity.Warning;

                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "VMF001",
                        "VMF Model Analysis",
                        diag.Message,
                        "VMF.NET",
                        severity,
                        isEnabledByDefault: true),
                    Location.None));
            }

            if (model.HasErrors) continue;

            // One registry per model namespace, so a type name can be resolved back to its type
            // object. Java uses Class.forName for this; a registry avoids runtime reflection and
            // also answers Reflect().AllTypes(). A module initialiser runs it on assembly load,
            // which matters because nothing else would guarantee the registration happened before
            // the first lookup.
            try
            {
                context.AddSource($"{ns}.__VmfTypeRegistration.g.cs", RenderRegistry(ns, model));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "VMF003",
                        "VMF Code Generation Error",
                        $"Error generating the type registry for '{ns}': {ex.Message}",
                        "VMF.NET",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None));
            }

            // Generate code for each type
            foreach (var type in model.Types)
            {
                try
                {
                    foreach (var (fileName, source) in renderer.RenderType(type, model))
                    {
                        context.AddSource($"{ns}.{fileName}", source);
                    }
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "VMF002",
                            "VMF Code Generation Error",
                            $"Error generating code for '{type.FullTypeName}': {ex.Message}",
                            "VMF.NET",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None));
                }
            }
        }
    }

    /// <summary>
    /// Emits the per-namespace type registration. Interface-only types are registered too -- they
    /// have no prototype factory, so reflecting on one throws, but they must still resolve by name
    /// when they appear as a super type.
    /// </summary>
    private static string RenderRegistry(string ns, VMF.NET.Core.ModelInfo model)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("// This code was generated by VMF.NET. Do not edit manually.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using VMF.NET.Runtime;");
        sb.AppendLine("using VMF.NET.Runtime.Internal;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns}");
        sb.AppendLine("{");
        sb.AppendLine("    internal static class __VmfTypeRegistration");
        sb.AppendLine("    {");
        sb.AppendLine("        [System.Runtime.CompilerServices.ModuleInitializer]");
        sb.AppendLine("        internal static void Register()");
        sb.AppendLine("        {");
        foreach (var type in model.Types)
        {
            if (type.IsInterfaceOnly)
            {
                sb.AppendLine($"            VmfTypeRegistry.Register(\"{type.FullTypeName}\", " +
                              $"VmfType.Create(true, false, true, \"{type.FullTypeName}\"));");
            }
            else
            {
                sb.AppendLine($"            VmfTypeRegistry.Register(\"{type.FullTypeName}\", " +
                              $"VmfType.Create(true, false, false, \"{type.FullTypeName}\", " +
                              $"static () => {type.TypeName}.NewInstance()));");
            }
        }
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
