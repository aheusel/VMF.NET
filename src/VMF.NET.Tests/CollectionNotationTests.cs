// How a model declares a collection: as an array, exactly as in Java VMF.
//
// The model writes `IElement[] Elements { get; }` and the generator produces a VList property.
// The point is that the model never names the collection type, so the generated API is free to
// change it without breaking anything written against it.
//
// See devdoc/differences-to-java.md, "Collections".

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;

namespace VMF.NET.Tests;

public class CollectionNotationTests
{
    private static GeneratorDriverRunResult Run(string source)
    {
        var refs = new List<MetadataReference>();
        var dir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        foreach (var name in new[]
                 {
                     "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll",
                     "System.ObjectModel.dll", "netstandard.dll",
                 })
        {
            var path = Path.Combine(dir, name);
            if (File.Exists(path)) refs.Add(MetadataReference.CreateFromFile(path));
        }
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "CollectionNotation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new VmfSourceGenerator());
        return driver.RunGenerators(compilation).GetRunResult();
    }

    private static string GeneratedInterface(GeneratorDriverRunResult result, string fileName) =>
        result.GeneratedTrees.Single(t => t.FilePath.EndsWith(fileName)).GetText().ToString();

    [Fact]
    public void ArrayOfModelType_BecomesAVListProperty()
    {
        var result = Run(@"
using VMF.NET.Runtime.Attributes;

namespace MyApp.VmfModel
{
    interface IParent
    {
        [Contains(""IChild.Parent"")] IChild[] Children { get; }
    }

    interface IChild
    {
        [Container(""IParent.Children"")] IParent? Parent { get; }
    }
}");

        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var parent = GeneratedInterface(result, "MyApp.IParent.g.cs");

        // Assert.Contains hides the haystack; a generator test needs to show what it produced.
        // The property itself must be a VList -- note the builder legitimately keeps an array in
        // its `params IChild[] values` overloads, so a blanket "no IChild[] anywhere" would be
        // wrong. Pin the property declaration exactly.
        Assert.True(parent.Contains("VList<IChild> Children { get; }"), parent);
        Assert.False(parent.Contains("IChild[] Children"), parent);
    }

    [Fact]
    public void ArrayOfPrimitive_BecomesAVListProperty()
    {
        var result = Run(@"
namespace MyApp.VmfModel
{
    interface IDevice
    {
        string[] Tags { get; }
    }
}");

        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var device = GeneratedInterface(result, "MyApp.IDevice.g.cs");
        Assert.True(device.Contains("VList<string> Tags { get; }"), device);
    }

    [Fact]
    public void NamingTheCollectionType_IsAnError()
    {
        // Reported rather than tolerated. Without the diagnostic this property would fall through
        // to a plain reference type and generate a single-valued property -- a silent miscompile
        // of a model written against the superseded rule.
        var result = Run(@"
using VMF.NET.Runtime;

namespace MyApp.VmfModel
{
    interface IBox
    {
        VList<IBoxItem> Items { get; }
    }

    interface IBoxItem
    {
        string? Name { get; set; }
    }
}");

        var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        var message = string.Join("; ", errors.Select(d => d.GetMessage()));
        Assert.True(errors.Count > 0, "expected an error, got none");
        Assert.True(message.Contains("VList<IBoxItem>"), message);

        // The message must say what to write instead, not merely that this is wrong.
        Assert.True(message.Contains("IBoxItem[]"), message);
    }

    [Fact]
    public void AMethodReturningAVList_IsLeftAlone()
    {
        // Arrays are the notation for PROPERTIES. A delegated method's return type is passed
        // through as written -- Java does the same, and its own MiniClangModel has
        // `//ControlFlowScope[] parentScopes();` commented out directly above the VList form.
        var result = Run(@"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace MyApp
{
    public class ScopeDelegate : IDelegatedBehavior<MyApp.IScoped>
    {
        public MyApp.IScoped Caller { get; set; } = null!;
        public void SetCaller(MyApp.IScoped caller) => Caller = caller;
        public VList<MyApp.IScoped>? ParentScopes() => null;
    }
}

namespace MyApp.VmfModel
{
    [DelegateTo(typeof(MyApp.ScopeDelegate))]
    interface IScoped
    {
        string? Name { get; set; }
        VList<IScoped>? ParentScopes();
    }
}");

        var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.True(errors.Count == 0, string.Join("; ", errors.Select(d => d.GetMessage())));
    }
}
