// Repro model for Issue A + B on the IMMUTABLE path (the real use case: a frozen capture
// document whose payloads are immutable value objects).
//
// An immutable base (Shape) with concrete subtypes (Circle, Rectangle), held in a
// heterogeneous value list on Drawing. Immutable element lists need no [Contains].

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface Shape
{
    string? Label { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface Circle : Shape
{
    double Radius { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface Rectangle : Shape
{
    double Width { get; }
    double Height { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface Drawing
{
    string? Title { get; }
    Shape[] Shapes { get; }
}
