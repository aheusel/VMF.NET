// Model interfaces for testing immutable types and required properties.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

/// <summary>
/// An immutable 2D point.
/// </summary>
[Immutable]
[VmfModel(Equality = EqualsType.All)]
interface Point
{
    double X { get; }
    double Y { get; }
}

/// <summary>
/// A mutable figure with a required name and points.
/// (Renamed from Shape to avoid colliding with the immutable Shape acceptance model.)
/// </summary>
[VmfModel(Equality = EqualsType.All)]
interface Figure
{
    [VmfRequired]
    string Name { get; set; }

    Point[] Points { get; }
}
