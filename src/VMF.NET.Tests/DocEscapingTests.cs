// Regression coverage for AGENT_FINDINGS.md Finding 2 —
// "Documentation text emitted raw into XML doc comments".
//
// [Doc(...)] text flows verbatim into /// <summary> comments in the generated read-only
// interface. Two hazards:
//   * a newline breaks out of the /// comment, emitting stray text as code (a compile ERROR), and
//   * <, >, & produce malformed XML doc comments and corrupt the rendered docs.
// We run the generator over model SOURCE TEXT, assert the output compiles (catches the newline
// break-out), and inspect the generated read-only interface to confirm XML-special chars are
// escaped and the newline is collapsed to a single space. Fails on the unpatched template,
// passes with the xml_doc fix.
//
// Harness mirrors VMF.NET.Tests/GeneratorCompilesTests.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VMF.NET.SourceGenerator;
using Xunit;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace VMF.NET.Tests;

public class DocEscapingTests
{
    [Fact]
    public void DocTextWithXmlSpecialCharsAndNewlines_GeneratesValidReadOnlyInterface()
    {
        // [Doc] on both the type and a property, each value carrying <, >, & and a newline (\n).
        const string source = @"
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;
namespace DocEscapingModel.VmfModel
{
    [VmfModel(Equality = EqualsType.All)]
    [Doc(""Maps Foo<Bar> & Baz.\nSecond line."")]
    interface IWidget
    {
        [Doc(""The <b>name</b> & label.\nMore text."")]
        string? Name { get; set; }
    }
}";

        var (generated, compileErrors) = RunGenerator(source);

        // The newline case, unescaped, breaks out of the /// comment and emits stray code,
        // so a clean compile already proves the newline is handled.
        Assert.True(compileErrors.Count == 0,
            "Generated code has compile error(s):\n" + string.Join("\n",
                compileErrors.Select(d => $"  {d.Id}: {d.GetMessage()} at {d.Location.GetLineSpan()}")));

        var roInterface = generated.FirstOrDefault(g => g.Contains("interface IReadOnlyWidget"));
        Assert.NotNull(roInterface);

        // XML-special chars escaped; newline collapsed to a single space (summary stays one line).
        Assert.Contains("Maps Foo&lt;Bar&gt; &amp; Baz. Second line.", roInterface);
        Assert.Contains("The &lt;b&gt;name&lt;/b&gt; &amp; label. More text.", roInterface);
        // The raw, unescaped form must not survive in the doc text.
        Assert.DoesNotContain("Foo<Bar>", roInterface);
    }

    // --- harness (same approach as GeneratorCompilesTests) ---

    private static (List<string> generated, List<Diagnostic> compileErrors) RunGenerator(string modelSource)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(modelSource);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "DocEscapingAcceptance",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new VmfSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var generated = driver.GetRunResult().GeneratedTrees.Select(t => t.GetText().ToString()).ToList();
        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        return (generated, compileErrors);
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
        // VMF.NET.Runtime supplies , [Doc], VList<T>, EqualsType, IVObject.
        refs.Add(MetadataReference.CreateFromFile(typeof(VMF.NET.Runtime.IVObject).Assembly.Location));
        return refs;
    }
}
