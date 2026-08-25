// Regression coverage for AGENT_FINDINGS.md Finding 1 —
// "Annotation key/value emitted without C# escaping".
//
// WHY THIS SHAPE: the defect is a *code-generation* bug. When an annotation value contains a
// double quote, a backslash, or a newline, the generator used to drop the decoded text verbatim
// into a "..." literal, so the emitted literal terminated early and the consuming build failed
// with a CS1003/CS0103/CS1729 cascade. A behavioral fact cannot catch that — broken generated
// code fails the whole assembly's build before any fact runs. So we run the generator over model
// SOURCE TEXT and assert the generated output compiles with no errors. This file itself always
// compiles (the model under test is a string), so it can fail its assertion on an unpatched
// generator and pass once csharp_string escaping is in place.
//
// Harness mirrors VMF.NET.Tests/GeneratorCompilesTests.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.Tests;

public class AnnotationEscapingTests
{
    // Exercises BOTH annotation emitters in Implementation.sbn:
    //   * the type-level emitter (_VMF_OBJECT_ANNOTATIONS) via the annotation on IDevice, and
    //   * the property-level emitter (_VMF_PROPERTY_ANNOTATIONS) via the ones on DeviceIds/Name.
    // Values chosen to hit every hazard: embedded double quotes (the exact reported repro),
    // a backslash, and a real newline (\n) in the decoded value.
    [Fact]
    public void AnnotationValuesWithQuotesBackslashesAndNewlines_GenerateCompilableCode()
    {
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace AnnotationEscapingModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)]
    [VmfAnnotation(""default=[\""alldrop\"", \""smartdrop\"", \""biospot\""]"", Key = ""vmf:schema:constraint"")]
    interface IDevice
    {
        // The exact value from the reported build failure — embedded double quotes.
        [VmfAnnotation(""default=[\""alldrop\"", \""smartdrop\"", \""biospot\""]"", Key = ""vmf:schema:constraint"")]
        string[] DeviceIds { get; }

        // Backslash, embedded quotes, and a real newline in the decoded value.
        [VmfAnnotation(""path\\to\nnext \""line\"""", Key = ""vmf:schema:description"")]
        string? Name { get; set; }
    }
}";
        AssertGeneratedCodeCompiles(source);
    }

    // --- harness (same approach as GeneratorCompilesTests) ---

    private static void AssertGeneratedCodeCompiles(string modelSource)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(modelSource);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "AnnotationEscapingAcceptance",
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

        // 2. The GENERATED code must compile (this is where the unescaped annotation used to fail).
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
        // VMF.NET.Runtime supplies , [VmfAnnotation], VList<T>, EqualsType, IVObject.
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));
        return refs;
    }
}
