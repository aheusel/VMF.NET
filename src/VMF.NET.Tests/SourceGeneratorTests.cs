// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.Core;
using VMF.NET.SourceGenerator;
using Xunit;

using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.Tests;

/// <summary>
/// Integration tests for the VMF.NET Source Generator.
/// Tests template rendering and full Roslyn generator pipeline.
/// </summary>
public class SourceGeneratorTests
{
    /// <summary>
    /// Tests that the template renderer produces valid code for a simple model.
    /// </summary>
    [Fact]
    public void TemplateRenderer_SimpleModel_ProducesValidCode()
    {
        // Arrange: create a simple model with two types
        var interfaces = new List<TypeSymbolData>
        {
            new()
            {
                Name = "IParent",
                FullName = "Test.Models.IParent",
                IsInterface = true,
                Properties = new()
                {
                    new()
                    {
                        Name = "Name",
                        FullTypeName = "System.String",
                        SimpleTypeName = "string",
                        TypeNamespace = "System",
                    },
                    new()
                    {
                        Name = "Age",
                        FullTypeName = "System.Int32",
                        SimpleTypeName = "int",
                        TypeNamespace = "System",
                        IsPrimitive = true,
                    },
                    new()
                    {
                        Name = "Children",
                        FullTypeName = "VList<Test.Models.IChild>",
                        SimpleTypeName = "VList",
                        IsCollection = true,
                        CollectionElementSimpleName = "IChild",
                        CollectionElementNamespace = "Test.Models",
                        ContainsOpposite = "IChild.Parent",
                    },
                },
            },
            new()
            {
                Name = "IChild",
                FullName = "Test.Models.IChild",
                IsInterface = true,
                Properties = new()
                {
                    new()
                    {
                        Name = "Title",
                        FullTypeName = "System.String",
                        SimpleTypeName = "string",
                        TypeNamespace = "System",
                    },
                    new()
                    {
                        Name = "Parent",
                        FullTypeName = "Test.Models.IParent",
                        SimpleTypeName = "IParent",
                        TypeNamespace = "Test.Models",
                        ContainerOpposite = "IParent.Children",
                    },
                },
            },
        };

        // Act
        var model = ModelAnalyzer.Analyze("Test.Models", interfaces);
        Assert.False(model.HasErrors, string.Join("\n", model.Diagnostics));

        var renderer = new TemplateRenderer();
        var generatedFiles = new Dictionary<string, string>();
        foreach (var type in model.Types)
        {
            foreach (var (fileName, source) in renderer.RenderType(type, model))
            {
                generatedFiles[fileName] = source;
            }
        }

        // Assert: files were generated
        Assert.True(generatedFiles.Count >= 8, $"Expected at least 8 generated files, got {generatedFiles.Count}");

        // Verify key files exist
        Assert.Contains("IParent.g.cs", generatedFiles.Keys);
        Assert.Contains("IReadOnlyParent.g.cs", generatedFiles.Keys);
        Assert.Contains("ParentImpl.g.cs", generatedFiles.Keys);
        Assert.Contains("ReadOnlyParentImpl.g.cs", generatedFiles.Keys);
        Assert.Contains("IChild.g.cs", generatedFiles.Keys);
        Assert.Contains("ChildImpl.g.cs", generatedFiles.Keys);

        // Verify basic content
        var parentImpl = generatedFiles["ParentImpl.g.cs"];
        Assert.Contains("class ParentImpl", parentImpl);
        Assert.Contains("public string?", parentImpl); // Name property
        Assert.Contains("public int Age", parentImpl);
        Assert.Contains("VList<IChild>", parentImpl); // Children collection
        Assert.Contains("UnregisterFromContainers", parentImpl);
        Assert.Contains("ChangesManager", parentImpl);

        var parentIface = generatedFiles["IParent.g.cs"];
        Assert.Contains("interface IParent", parentIface);
        Assert.Contains("Builder", parentIface);
        Assert.Contains("NewBuilder()", parentIface);
        Assert.Contains("NewInstance()", parentIface);

        var childImpl = generatedFiles["ChildImpl.g.cs"];
        Assert.Contains("class ChildImpl", childImpl);
        Assert.Contains("__vmf_container", childImpl); // Container reference

        var roIface = generatedFiles["IReadOnlyParent.g.cs"];
        Assert.Contains("interface IReadOnlyParent", roIface);
        Assert.Contains("IReadOnlyChild", roIface); // Collection element mapped to read-only

        var roImpl = generatedFiles["ReadOnlyParentImpl.g.cs"];
        Assert.Contains("class ReadOnlyParentImpl", roImpl);
        Assert.Contains("IsReadOnly => true", roImpl);
    }

    /// <summary>
    /// Tests that generated code for a simple scalar-only model compiles successfully.
    /// </summary>
    [Fact]
    public void TemplateRenderer_ScalarModel_CompilesSuccessfully()
    {
        var interfaces = new List<TypeSymbolData>
        {
            new()
            {
                Name = "INode",
                FullName = "TestModel.INode",
                IsInterface = true,
                Properties = new()
                {
                    new()
                    {
                        Name = "Name",
                        FullTypeName = "System.String",
                        SimpleTypeName = "string",
                        TypeNamespace = "System",
                    },
                    new()
                    {
                        Name = "Value",
                        FullTypeName = "System.Int32",
                        SimpleTypeName = "int",
                        TypeNamespace = "System",
                        IsPrimitive = true,
                    },
                },
            },
        };

        var model = ModelAnalyzer.Analyze("TestModel", interfaces);
        Assert.False(model.HasErrors, string.Join("\n", model.Diagnostics));

        var renderer = new TemplateRenderer();
        var sources = new List<string>();
        foreach (var type in model.Types)
        {
            foreach (var (_, source) in renderer.RenderType(type, model))
            {
                sources.Add(source);
            }
        }

        // Add a "user source" that declares the original interface with properties
        var userSource = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace TestModel
{
    public partial interface INode
    {
        string? Name { get; set; }
        int Value { get; set; }
    }
}";
        sources.Insert(0, userSource);

        // Try to compile the generated code
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (diagnostics.Count > 0)
        {
            var errorMessages = string.Join("\n", diagnostics.Select(d =>
                $"  {d.Id}: {d.GetMessage()} at {d.Location.GetLineSpan()}"));
            var allSource = string.Join("\n\n// --- NEXT FILE ---\n\n", sources);
            Assert.Fail($"Generated code has {diagnostics.Count} compilation error(s):\n{errorMessages}\n\n--- Generated Source ---\n{allSource}");
        }
    }

    /// <summary>
    /// Tests content-based equals template generation.
    /// </summary>
    [Fact]
    public void TemplateRenderer_ContentEquals_GeneratesEqualsCode()
    {
        var interfaces = new List<TypeSymbolData>
        {
            new()
            {
                Name = "IPoint",
                FullName = "Geom.IPoint",
                IsInterface = true,
                VmfEqualsAttribute = new VmfEqualsData { Value = EqualsStrategy.All },
                Properties = new()
                {
                    new()
                    {
                        Name = "X",
                        FullTypeName = "System.Double",
                        SimpleTypeName = "double",
                        TypeNamespace = "System",
                        IsPrimitive = true,
                    },
                    new()
                    {
                        Name = "Y",
                        FullTypeName = "System.Double",
                        SimpleTypeName = "double",
                        TypeNamespace = "System",
                        IsPrimitive = true,
                    },
                },
            },
        };

        var model = ModelAnalyzer.Analyze("Geom", interfaces);
        Assert.False(model.HasErrors);

        var renderer = new TemplateRenderer();
        string? implSource = null;
        foreach (var (fileName, source) in renderer.RenderType(model.Types[0], model))
        {
            if (fileName == "PointImpl.g.cs") implSource = source;
        }

        Assert.NotNull(implSource);
        Assert.Contains("override bool Equals", implSource);
        Assert.Contains("override int GetHashCode", implSource);
        Assert.Contains("VmfEquals", implSource);
        Assert.Contains("VmfHashCode", implSource);
    }

    /// <summary>
    /// Tests the full Roslyn generator pipeline end-to-end.
    /// </summary>
    [Fact]
    public void FullPipeline_SimpleModel_GeneratesAndCompiles()
    {
        var source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace MyModel
{
    [VmfModel]
    public partial interface IItem
    {
        string Name { get; set; }
        int Count { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "TestGen",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new VmfSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var genDiagnostics);

        // Check generator didn't error
        var genErrors = genDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(genErrors);

        // Check generated output compiles
        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (compileErrors.Count > 0)
        {
            var runResult = driver.GetRunResult();
            var generatedSources = runResult.GeneratedTrees.Select(t => t.GetText().ToString()).ToList();
            var errorMessages = string.Join("\n", compileErrors.Select(d =>
                $"  {d.Id}: {d.GetMessage()} at {d.Location.GetLineSpan()}"));
            var allGenerated = string.Join("\n---\n", generatedSources);
            Assert.Fail($"Generated code has compilation errors:\n{errorMessages}\n\n--- Generated ---\n{allGenerated}");
        }

        // Verify something was generated
        var result = driver.GetRunResult();
        Assert.True(result.GeneratedTrees.Length > 0, "Expected generated source trees");
    }

    /// <summary>
    /// Tests builder generation.
    /// </summary>
    [Fact]
    public void TemplateRenderer_Builder_HasFluentMethods()
    {
        var interfaces = new List<TypeSymbolData>
        {
            new()
            {
                Name = "IConfig",
                FullName = "App.IConfig",
                IsInterface = true,
                Properties = new()
                {
                    new()
                    {
                        Name = "Host",
                        FullTypeName = "System.String",
                        SimpleTypeName = "string",
                        TypeNamespace = "System",
                    },
                    new()
                    {
                        Name = "Port",
                        FullTypeName = "System.Int32",
                        SimpleTypeName = "int",
                        TypeNamespace = "System",
                        IsPrimitive = true,
                    },
                },
            },
        };

        var model = ModelAnalyzer.Analyze("App", interfaces);
        Assert.False(model.HasErrors);

        var renderer = new TemplateRenderer();
        string? implSource = null;
        foreach (var (fileName, source) in renderer.RenderType(model.Types[0], model))
        {
            if (fileName == "ConfigImpl.g.cs") implSource = source;
        }

        Assert.True(implSource != null, "ConfigImpl.g.cs not found");
        Assert.Contains("class BuilderImpl", implSource);
        Assert.Contains("WithHost(", implSource);
        Assert.Contains("WithPort(", implSource);
        Assert.Contains("Build()", implSource);
        Assert.Contains("ApplyFrom(", implSource);
        Assert.Contains("ApplyTo(", implSource);
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var refs = new List<MetadataReference>();

        // Core runtime assemblies
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Linq.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.ObjectModel.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.ComponentModel.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.ComponentModel.Primitives.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")));

        // VMF.NET.Runtime
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));

        return refs;
    }
}
