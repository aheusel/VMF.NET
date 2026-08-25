// TIER 1 — compile-gate acceptance for Issue A (inheritance) and Issue C (nullable value types).
//
// WHY THIS SHAPE: A and C are *code-generation* defects — the emitted code does not compile.
// A behavioral xUnit fact in the consuming assembly cannot test that: the broken generated
// code fails the whole assembly's build, so no fact ever runs. Instead we run the generator
// over model SOURCE TEXT and assert the generated output compiles with no diagnostics. This
// test file itself always compiles (the model under test is a string), so it can fail its
// assertion today (CS0738/CS0539/CS0723) and pass after the fix.
//
// Harness mirrors VMF.NET.Tests/SourceGeneratorTests.FullPipeline_SimpleModel_GeneratesAndCompiles.
// DROP INTO: src/VMF.NET.Tests/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.Tests;

public class GeneratorCompilesTests
{
    [Fact]
    public void IssueA_MutableInheritanceHierarchy_GeneratesCompilableCode()
    {
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace InheritanceModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)]
    interface IAnimal
    {
        string? Name { get; set; }
        int Age { get; set; }
        [Container(""IZoo.Animals"")] IZoo? Zoo { get; }
    }

    [VmfModel(Equality = EqualsType.All)]
    interface IDog : IAnimal { string? Breed { get; set; } }

    [VmfModel(Equality = EqualsType.All)]
    interface ICat : IAnimal { bool Indoor { get; set; } }

    [VmfModel(Equality = EqualsType.All)]
    interface IZoo
    {
        string? Name { get; set; }
        [Contains(""IAnimal.Zoo"")] VList<IAnimal> Animals { get; }
    }
}";
        AssertGeneratedCodeCompiles(source);
    }

    [Fact]
    public void IssueA_ImmutableInheritanceHierarchy_GeneratesCompilableCode()
    {
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace ImmutableModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)] [Immutable]
    interface IShape { string? Label { get; } }

    [VmfModel(Equality = EqualsType.All)] [Immutable]
    interface ICircle : IShape { double Radius { get; } }

    [VmfModel(Equality = EqualsType.All)] [Immutable]
    interface IRectangle : IShape { double Width { get; } double Height { get; } }

    [VmfModel(Equality = EqualsType.All)] [Immutable]
    interface IDrawing { string? Title { get; } VList<IShape> Shapes { get; } }
}";
        AssertGeneratedCodeCompiles(source);
    }

    [Fact]
    public void IssueC_NullableValueTypeProperties_GenerateCompilableCode()
    {
        const string source = @"
using VMF.NET.Runtime.Attributes;
namespace NullableModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)]
    interface IMeasurement
    {
        string? Label { get; set; }
        double? Value { get; set; }
        int? Count { get; set; }
        bool? Flag { get; set; }
    }
}";
        AssertGeneratedCodeCompiles(source);
    }

    [Fact]
    public void InterfaceOnlyBase_GeneratesCompilableCode()
    {
        // An [InterfaceOnly] type never declares Clone()/AsReadOnly() (Interface.sbn skips them),
        // so the implementation must NOT emit explicit interface implementations for such a base
        // -- doing so produced CS0539. Covers the mutable and the immutable derived case.
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace InterfaceOnlyModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)] [InterfaceOnly]
    interface IWithName { string? Name { get; set; } }

    [VmfModel(Equality = EqualsType.All)]
    interface IThing : IWithName { int Value { get; set; } }

    [VmfModel(Equality = EqualsType.All)] [InterfaceOnly]
    interface IWithLabel { [GetterOnly] string? Label { get; } }

    [VmfModel(Equality = EqualsType.All)] [Immutable]
    interface IFrozen : IWithLabel { int Size { get; } }
}";
        AssertGeneratedCodeCompiles(source);
    }

    [Fact]
    public void GetterOnlyProperty_OnMutableType_GeneratesCompilableCode()
    {
        // A [GetterOnly] property generates no public setter, so SetPropertyValueById and the
        // builder's Build() must assign the backing field instead -- assigning the property
        // produced CS0200. ApplyTo() cannot set it at all and must skip it.
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace GetterOnlyModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)] [InterfaceOnly]
    interface IWithName { [GetterOnly] string? Name { get; } }

    [VmfModel(Equality = EqualsType.All)]
    interface IThing : IWithName { int Value { get; set; } }
}";
        AssertGeneratedCodeCompiles(source);
    }

    // --- harness (same approach as SourceGeneratorTests.FullPipeline_*) ---

    private static void AssertGeneratedCodeCompiles(string modelSource)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(modelSource);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "GenAcceptance",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new VmfSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var genDiagnostics);

        // 1. The generator itself must not error.
        var genErrors = genDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.True(genErrors.Count == 0,
            "Generator reported errors:\n" + string.Join("\n", genErrors.Select(d => $"  {d.Id}: {d.GetMessage()}")));

        // 1b. Something must actually have been generated. Without this the whole assertion is
        // vacuous when the model is not recognised -- a model source in the wrong namespace
        // generates nothing, and "nothing" compiles perfectly.
        Assert.True(driver.GetRunResult().GeneratedTrees.Length > 0,
            "The generator produced no output. Is the model in a '.VmfModel' namespace?");

        // 2. The GENERATED code must compile (this is where A/C currently fail).
        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (compileErrors.Count > 0)
        {
            var generated = string.Join("\n// --- next generated file ---\n",
                driver.GetRunResult().GeneratedTrees.Select(t => t.GetText().ToString()));
            var messages = string.Join("\n", compileErrors.Select(d =>
                $"  {d.Id}: {d.GetMessage()} at {d.Location.GetLineSpan()}"));
            Assert.Fail($"Generated code has {compileErrors.Count} compile error(s):\n{messages}\n\n--- Generated ---\n{generated}");
        }
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var refs = new List<MetadataReference>();
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        foreach (var dll in new[]
                 {
                     "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll",
                     "System.ObjectModel.dll", "System.ComponentModel.dll",
                     "System.ComponentModel.Primitives.dll", "netstandard.dll",
                 })
        {
            refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, dll)));
        }
        // VMF.NET.Runtime supplies , [Contains]/[Container], [Immutable], VList<T>, EqualsType, IVObject.
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));
        return refs;
    }
}
