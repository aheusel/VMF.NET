// Ported from eu.mihosoft.vmftest.propertyorder.vmfmodel.DefaultOrder
//
// DEVIATION: Java's BaseClass/Inherited pair redeclares getValue() changing the type from
// Object to Integer. C# interfaces cannot narrow a property type on redeclaration without
// hiding it, and the generator emits a single implementation, so the base member would be
// left unimplemented. The type is kept as object? on both here; the redeclared-order
// scenario (the point of the pair) is preserved.

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyOrder;

[VmfModel]
public partial interface IDefaultOrder
{
    string? Z { get; set; }
    IElement? B { get; set; }
    int? D { get; set; }
    double X { get; set; }
}

[VmfModel]
public partial interface ICustomOrder
{
    [PropertyOrder(3)] int? D { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] IElement? B { get; set; }
    [PropertyOrder(4)] double X { get; set; }
}

[VmfModel]
public partial interface IElement
{
}

[VmfModel]
public partial interface IInheritedBaseWithoutCustomOrder
{
    string? BaseA { get; set; }
    string? BaseZ { get; set; }
    string? BaseB { get; set; }
}

[VmfModel]
public partial interface IInheritedOrderSubClassWithoutBaseOrder : IInheritedBaseWithoutCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

[VmfModel]
public partial interface IInheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? BaseA { get; set; }
    [PropertyOrder(1)] string? BaseZ { get; set; }
    [PropertyOrder(2)] string? BaseB { get; set; }
}

[VmfModel]
public partial interface IInheritedOrderSubClassWithBaseOrder : IInheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

[VmfModel]
public partial interface IInheritedOrderSubClassWithRedefinedBaseOrder : IInheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? Z { get; set; }
    [PropertyOrder(1)] new string? B { get; set; }
    [PropertyOrder(2)] new string? A { get; set; }
}

// redeclare property order unchanged -> compile-only test
[VmfModel]
public partial interface IInheritedOrderSubClassWithRedefinedBaseOrderUnchanged : IInheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? A { get; set; }
    [PropertyOrder(1)] new string? Z { get; set; }
    [PropertyOrder(2)] new string? B { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IBaseClass
{
    [PropertyOrder(0)]
    [GetterOnly]
    object? Value { get; }
}

[VmfModel]
public partial interface IInherited : IBaseClass
{
    // Java narrows this to Integer; see the DEVIATION note at the top of the file.
    [PropertyOrder(0)]
    new object? Value { get; set; }
}
