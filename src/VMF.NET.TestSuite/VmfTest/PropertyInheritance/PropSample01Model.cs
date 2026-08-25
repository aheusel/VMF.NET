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
interface WithX
{
    [GetterOnly] double X { get; }
}

[InterfaceOnly]
interface WithY
{
    [GetterOnly] double Y { get; }
}

[InterfaceOnly]
interface WithXY : WithX, WithY
{
}

interface Location
{
}

interface LocationX : Location, WithX
{
}

interface LocationY : Location, WithY
{
}

interface LocationXY : WithXY, LocationX, LocationY
{
}

[InterfaceOnly]
interface WithLocation
{
    [GetterOnly] Location? Location { get; }
}

[InterfaceOnly]
interface WithLocationX : WithLocation
{
    [GetterOnly] new LocationX? Location { get; }
}

[InterfaceOnly]
interface WithLocationY : WithLocation
{
    [GetterOnly] new LocationY? Location { get; }
}

[InterfaceOnly]
interface WithLocationXY : WithLocationX, WithLocationY
{
    [GetterOnly] new LocationXY? Location { get; }
}

interface PropSample01
{
    [Contains] GCode1? GCode1 { get; set; }
    [Contains] GCode2? GCode2 { get; set; }
    [Contains] GCode3? GCode3 { get; set; }
    [Contains] GCode4? GCode4 { get; set; }
}

interface GCode1 : WithLocationXY
{
    new LocationXY? Location { get; set; }
}

interface GCode2 : WithLocationX
{
    new LocationX? Location { get; set; }
}

interface GCode3 : WithLocationY
{
    new LocationY? Location { get; set; }
}

interface GCode4 : WithLocation
{
    new Location? Location { get; set; }
}
