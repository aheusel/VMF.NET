// Ported from eu.mihosoft.vmftest.propertyorder.vmfmodel.DefaultOrder

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyOrder.VmfModel;

interface IDefaultOrder
{
    string? Z { get; set; }
    IElement? B { get; set; }
    int? D { get; set; }
    double X { get; set; }
}

interface ICustomOrder
{
    [PropertyOrder(3)] int? D { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] IElement? B { get; set; }
    [PropertyOrder(4)] double X { get; set; }
}

interface IElement
{
}

interface IInheritedBaseWithoutCustomOrder
{
    string? BaseA { get; set; }
    string? BaseZ { get; set; }
    string? BaseB { get; set; }
}

interface IInheritedOrderSubClassWithoutBaseOrder : IInheritedBaseWithoutCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

interface IInheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? BaseA { get; set; }
    [PropertyOrder(1)] string? BaseZ { get; set; }
    [PropertyOrder(2)] string? BaseB { get; set; }
}

interface IInheritedOrderSubClassWithBaseOrder : IInheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

interface IInheritedOrderSubClassWithRedefinedBaseOrder : IInheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? Z { get; set; }
    [PropertyOrder(1)] new string? B { get; set; }
    [PropertyOrder(2)] new string? A { get; set; }
}

// redeclare property order unchanged -> compile-only test
interface IInheritedOrderSubClassWithRedefinedBaseOrderUnchanged : IInheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? A { get; set; }
    [PropertyOrder(1)] new string? Z { get; set; }
    [PropertyOrder(2)] new string? B { get; set; }
}

[InterfaceOnly]
interface IBaseClass
{
    [PropertyOrder(0)]
    [GetterOnly]
    object? Value { get; }
}

interface IInherited : IBaseClass
{
    // this should be allowed
    // -> until v0.2.6.1 it wasn't
    //    because of the property type change
    //    which is 'object' in the getter-only
    //    and 'int' here. that's where
    //    VMF checks for redeclared property
    //    order failed
    [PropertyOrder(0)]
    new int? Value { get; set; }
}
