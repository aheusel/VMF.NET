// Model interfaces for testing immutable types and required properties.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.IntegrationTests.Models;

/// <summary>
/// An immutable 2D point.
/// </summary>
[Immutable]
[VmfModel(Equality = EqualsType.All)]
public partial interface IPoint
{
    double X { get; }
    double Y { get; }
}

/// <summary>
/// A mutable shape with a required name and points.
/// </summary>
[VmfModel(Equality = EqualsType.All)]
public partial interface IShape
{
    [VmfRequired]
    string Name { get; set; }

    VList<IPoint> Points { get; }
}
