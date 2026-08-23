// Ported from eu.mihosoft.vmftest.propertyinheritance.vmfmodel.PropSample01
//
// DEVIATION: Java narrows getLocation() covariantly down the WithLocation chain
// (Location -> LocationX -> LocationY -> LocationXY). C# interfaces have no covariant
// property overriding: a narrowed redeclaration only hides the base member, and the
// generator emits one implementation, leaving the base member unimplemented. The chain
// therefore keeps ILocation as the property type; the inheritance shape (diamonds via
// WithXY / LocationXY / WithLocationXY) is preserved, which is what the area exercises.

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyInheritance;

[VmfModel]
[InterfaceOnly]
public partial interface IWithX
{
    [GetterOnly] double X { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithY
{
    [GetterOnly] double Y { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithXY : IWithX, IWithY
{
}

[VmfModel]
public partial interface ILocation
{
}

[VmfModel]
public partial interface ILocationX : ILocation, IWithX
{
}

[VmfModel]
public partial interface ILocationY : ILocation, IWithY
{
}

[VmfModel]
public partial interface ILocationXY : IWithXY, ILocationX, ILocationY
{
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithLocation
{
    [GetterOnly] ILocation? Location { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithLocationX : IWithLocation
{
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithLocationY : IWithLocation
{
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithLocationXY : IWithLocationX, IWithLocationY
{
}

[VmfModel]
public partial interface IPropSample01
{
    [Contains] IGCode1? GCode1 { get; set; }
    [Contains] IGCode2? GCode2 { get; set; }
    [Contains] IGCode3? GCode3 { get; set; }
    [Contains] IGCode4? GCode4 { get; set; }
}

[VmfModel]
public partial interface IGCode1 : IWithLocationXY
{
}

[VmfModel]
public partial interface IGCode2 : IWithLocationX
{
}

[VmfModel]
public partial interface IGCode3 : IWithLocationY
{
}

[VmfModel]
public partial interface IGCode4 : IWithLocation
{
}
