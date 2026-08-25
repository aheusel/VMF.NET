// Repro model for Issue A + B on the IMMUTABLE path (the real use case: a frozen capture
// document whose payloads are immutable value objects).
//
// An immutable base (IShape) with concrete subtypes (ICircle, IRectangle), held in a
// heterogeneous value list on IDrawing. Immutable element lists need no [Contains].

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface IShape
{
    string? Label { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface ICircle : IShape
{
    double Radius { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface IRectangle : IShape
{
    double Width { get; }
    double Height { get; }
}

[VmfModel(Equality = EqualsType.All)]
[Immutable]
interface IDrawing
{
    string? Title { get; }
    IShape[] Shapes { get; }
}
