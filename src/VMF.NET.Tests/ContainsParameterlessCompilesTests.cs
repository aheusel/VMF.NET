// TIER 1 — compile-gate acceptance for parameterless [Contains] (opposite-less containment).
//
// WHY THIS SHAPE: the defect is compile-time — `[Contains]` (no args) fails CS7036 on the model
// source, and a behavioral xUnit fact in the consuming assembly cannot test that (the broken model
// fails the whole assembly's build, so no fact runs). Instead we run the generator over model
// SOURCE TEXT and assert the model + generated output compile with no diagnostics. This test file
// itself always compiles (the model under test is a string), so it can fail its assertion today
// (CS7036) and pass after the fix.
//
// Harness mirrors VMF.NET.Tests/SourceGeneratorTests.FullPipeline_SimpleModel_GeneratesAndCompiles.
// DROP INTO: src/VMF.NET.Tests/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.Tests;

public class ContainsParameterlessCompilesTests
{
    [Fact]
    public void ParameterlessContains_GeneratesCompilableCode()
    {
        // A container that owns its children via [Contains] with NO opposite, and a contained type
        // that declares NO [Container] back-reference. Today the [Contains] application fails with
        // CS7036; once ContainsAttribute exposes a parameterless ctor, the model compiles and the
        // generator's existing "without opposite" path produces compilable code.
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace ParameterlessContainsModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)]
    interface IBoxItem
    {
        string? Name { get; set; }
    }

    [VmfModel(Equality = EqualsType.All)]
    interface IBox
    {
        string? Label { get; set; }
        [Contains] VList<IBoxItem> Items { get; }
    }
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

        // 2. The MODEL + GENERATED code must compile. The model source fails here today with
        //    CS7036 (parameterless [Contains] is inexpressible); after the fix it compiles.
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
