// Probe/regression model for covariant property narrowing.
namespace VMF.NET.TestSuite.Models.VmfModel;

interface Glyph { string? Label { get; set; } }

interface Round : Glyph { double Radius { get; set; } }

interface Boxy : Glyph { double Side { get; set; } }

interface GlyphHolder
{
    Glyph? Value { get; set; }
    Glyph[] Values { get; }
}

interface RoundHolder : GlyphHolder
{
    new Round? Value { get; set; }

    // NOTE: `new Round[] Values { get; }` is deliberately absent -- narrowing a COLLECTION is a
    // build error (VMF001), because VList<T> is invariant and the base declaration could not be
    // implemented. NarrowingDiagnosticTests pins that message.
}
