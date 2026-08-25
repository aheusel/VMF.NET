// Ported from eu.mihosoft.vmftest.propertyinheritance.vmfmodel.PropSample01
//
// Location is narrowed down the WithLocation chain (Location -> LocationX / LocationY ->
// LocationXY), which is what this area is for. The `new` keywords are C#: narrowing a property
// on redeclaration hides the base member rather than overriding it, so the compiler asks for
// the intent to be stated. The generated implementation carries the narrowed member and
// satisfies each base interface with a forwarding explicit implementation.

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyInheritance.VmfModel;

[InterfaceOnly]
interface IWithX
{
    [GetterOnly] double X { get; }
}

[InterfaceOnly]
interface IWithY
{
    [GetterOnly] double Y { get; }
}

[InterfaceOnly]
interface IWithXY : IWithX, IWithY
{
}

interface ILocation
{
}

interface ILocationX : ILocation, IWithX
{
}

interface ILocationY : ILocation, IWithY
{
}

interface ILocationXY : IWithXY, ILocationX, ILocationY
{
}

[InterfaceOnly]
interface IWithLocation
{
    [GetterOnly] ILocation? Location { get; }
}

[InterfaceOnly]
interface IWithLocationX : IWithLocation
{
    [GetterOnly] new ILocationX? Location { get; }
}

[InterfaceOnly]
interface IWithLocationY : IWithLocation
{
    [GetterOnly] new ILocationY? Location { get; }
}

[InterfaceOnly]
interface IWithLocationXY : IWithLocationX, IWithLocationY
{
    [GetterOnly] new ILocationXY? Location { get; }
}

interface IPropSample01
{
    [Contains] IGCode1? GCode1 { get; set; }
    [Contains] IGCode2? GCode2 { get; set; }
    [Contains] IGCode3? GCode3 { get; set; }
    [Contains] IGCode4? GCode4 { get; set; }
}

interface IGCode1 : IWithLocationXY
{
    new ILocationXY? Location { get; set; }
}

interface IGCode2 : IWithLocationX
{
    new ILocationX? Location { get; set; }
}

interface IGCode3 : IWithLocationY
{
    new ILocationY? Location { get; set; }
}

interface IGCode4 : IWithLocation
{
    new ILocation? Location { get; set; }
}
