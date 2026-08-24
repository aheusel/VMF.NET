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
        // Step 1: Find all interface declarations that have at least one VMF attribute
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsVmfInterface(node),
                transform: static (ctx, _) => GetVmfInterfaceSymbol(ctx))
            .Where(static symbol => symbol != null)
            .Select(static (symbol, _) => symbol!);

        // Step 2: Combine all interfaces and the compilation
        var collected = interfaceDeclarations.Collect();

        // Step 3: Generate source
        context.RegisterSourceOutput(collected, static (spc, interfaces) => Execute(spc, interfaces));
    }

    /// <summary>
    /// Syntax predicate: is this an interface with at least one attribute?
    /// </summary>
    private static bool IsVmfInterface(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        if (node is not InterfaceDeclarationSyntax iface) return false;
        // Check interface-level attributes
        if (iface.AttributeLists.Count > 0) return true;
        // Check member-level attributes (properties with [Contains], [Refers], etc.)
        foreach (var member in iface.Members)
        {
            if (member is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax prop
                && prop.AttributeLists.Count > 0)
                return true;
            if (member is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method
                && method.AttributeLists.Count > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Semantic transform: resolve the symbol and check for VMF attributes.
    /// </summary>
    private static INamedTypeSymbol? GetVmfInterfaceSymbol(GeneratorSyntaxContext context)
    {
        var interfaceSyntax = (InterfaceDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(interfaceSyntax) is not INamedTypeSymbol symbol)
            return null;

        // Check if this interface has VMF attributes
        if (HasAnyVmfAttribute(symbol))
            return symbol;

        return null;
    }

    private static bool HasAnyVmfAttribute(INamedTypeSymbol symbol)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var name = attr.AttributeClass?.Name;
            if (name == null) continue;
            if (IsVmfAttributeName(name)) return true;
        }

        // Check if any property has VMF attributes
        foreach (var member in symbol.GetMembers())
        {
            if (member is IPropertySymbol prop)
            {
                foreach (var attr in prop.GetAttributes())
                {
                    var name = attr.AttributeClass?.Name;
                    if (name != null && IsVmfAttributeName(name)) return true;
                }
            }
        }

        return false;
    }

    private static bool IsVmfAttributeName(string name)
    {
        return name == "VmfModelAttribute" || name == "VmfModel"
            || name == "ContainsAttribute" || name == "Contains"
            || name == "ContainerAttribute" || name == "Container"
            || name == "RefersAttribute" || name == "Refers"
            || name == "ImmutableAttribute" || name == "Immutable"
            || name == "InterfaceOnlyAttribute" || name == "InterfaceOnly"
            || name == "ExternalTypeAttribute" || name == "ExternalType"
            || name == "VmfEqualsAttribute" || name == "VmfEquals"
            || name == "DelegateToAttribute" || name == "DelegateTo"
            || name == "VmfAnnotationAttribute" || name == "VmfAnnotation"
            || name == "RequiredAttribute" || name == "Required"
            || name == "VmfRequiredAttribute" || name == "VmfRequired"
            || name == "GetterOnlyAttribute" || name == "GetterOnly"
            || name == "IgnoreEqualsAttribute" || name == "IgnoreEquals"
            || name == "IgnoreToStringAttribute" || name == "IgnoreToString"
            || name == "DefaultValueAttribute" || name == "DefaultValue"
            || name == "VmfDefaultValueAttribute" || name == "VmfDefaultValue"
            || name == "PropertyOrderAttribute" || name == "PropertyOrder"
            || name == "DocAttribute" || name == "Doc";
    }

    /// <summary>
    /// Main generation logic.
    /// </summary>
    private static void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> interfaces)
    {
        if (interfaces.IsDefaultOrEmpty) return;

        // Group interfaces by namespace (each namespace forms a separate model)
        var byNamespace = new Dictionary<string, List<INamedTypeSymbol>>();
        foreach (var iface in interfaces)
        {
            var ns = iface.ContainingNamespace?.ToDisplayString() ?? "Global";
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
