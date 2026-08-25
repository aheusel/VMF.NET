// What makes an interface a model type: its namespace, and nothing else.
//
// The rule this replaced sniffed for any VMF attribute, and was unsound in both directions --
// it missed a plain model interface carrying no attribute, and it matched unrelated interfaces
// because attribute names were compared without their namespace. Both directions are pinned here.
//
// See devdoc/system_constraints.md, C-6.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;

namespace VMF.NET.Tests;

public class ModelDiscoveryTests
{
    private static GeneratorDriverRunResult Run(string source)
    {
        var refs = new List<MetadataReference>();
        var dir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        foreach (var name in new[]
                 {
                     "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll",
                     "System.ObjectModel.dll", "System.ComponentModel.dll",
                     "System.ComponentModel.Primitives.dll", "System.ComponentModel.Annotations.dll",
                     "netstandard.dll",
                 })
        {
            var path = Path.Combine(dir, name);
            if (File.Exists(path)) refs.Add(MetadataReference.CreateFromFile(path));
        }
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "Discovery",
            new[] { CSharpSyntaxTree.ParseText(source) },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new VmfSourceGenerator());
        return driver.RunGenerators(compilation).GetRunResult();
    }

    [Fact]
    public void InterfaceInAModelNamespace_NeedsNoAttributeAtAll()
    {
        // Java's Test2.Named is exactly this: a model type with nothing on it. Under the old rule
        // it was invisible, which is why [VmfModel] had to be written on almost every interface.
        var result = Run(@"
namespace MyApp.VmfModel
{
    interface Named
    {
        string Name { get; set; }
    }
}");

        var files = result.GeneratedTrees.Select(t => Path.GetFileName(t.FilePath)).ToList();

        Assert.Contains("MyApp.INamed.g.cs", files);
        Assert.Contains("MyApp.NamedImpl.g.cs", files);
    }

    [Fact]
    public void InterfaceOutsideAModelNamespace_GeneratesNothing()
    {
        // [Required] here is System.ComponentModel.DataAnnotations'. Matching attribute names
        // without their namespace read it as VMF's, and this generated five files including a
        // full implementation.
        var result = Run(@"
using System.ComponentModel.DataAnnotations;
namespace MyApp.NotAModelAtAll
{
    public interface ICustomerDto
    {
        [Required] string Name { get; set; }
    }
}");

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void ExternalTypeStandIn_ResolvesToTheTypeItNames()
    {
        // A stand-in is not a model type: it names something living outside the model, and the
        // generated code must reference THAT. Before this was fixed the attribute's namespace
        // argument was ignored entirely — generated code referenced the empty stand-in itself.
        var result = Run(@"
using VMF.NET.Runtime.Attributes;

namespace Other
{
    public class Payload { public string? Text { get; set; } }
}

namespace MyApp.VmfModel
{
    [ExternalType(""Other"")]
    interface Payload { }

    interface Holder
    {
        Payload? Cargo { get; set; }
    }
}");

        var files = result.GeneratedTrees.Select(t => Path.GetFileName(t.FilePath)).ToList();

        // The stand-in gets no implementation of its own.
        Assert.DoesNotContain("MyApp.IPayload.g.cs", files);
        Assert.DoesNotContain("MyApp.PayloadImpl.g.cs", files);

        var holder = result.GeneratedTrees.Single(t => t.FilePath.EndsWith("MyApp.IHolder.g.cs"));
        var text = holder.GetText().ToString();

        // Assert.Contains hides the haystack; a generator test needs to show what it produced.
        Assert.True(text.Contains("Other.Payload? Cargo"), text);
        Assert.False(text.Contains("IPayload"), text);
    }

    [Fact]
    public void ModelNameGainsTheInterfacePrefix_UnlessItAlreadyHasOne()
    {
        // Keeping the prefix when it is already there is what let every existing model migrate by
        // moving its namespace and nothing else.
        var result = Run(@"
namespace MyApp.VmfModel
{
    interface Parent { string Name { get; set; } }
    interface IChild { string Name { get; set; } }
}");

        var files = result.GeneratedTrees.Select(t => Path.GetFileName(t.FilePath)).ToList();

        Assert.Contains("MyApp.IParent.g.cs", files);
        Assert.Contains("MyApp.IChild.g.cs", files);
        Assert.DoesNotContain("MyApp.IIChild.g.cs", files);
    }
}
