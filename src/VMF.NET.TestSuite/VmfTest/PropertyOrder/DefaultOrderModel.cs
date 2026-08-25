// Ported from eu.mihosoft.vmftest.propertyorder.vmfmodel.DefaultOrder

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyOrder.VmfModel;

interface DefaultOrder
{
    string? Z { get; set; }
    Element? B { get; set; }
    int? D { get; set; }
    double X { get; set; }
}

interface CustomOrder
{
    [PropertyOrder(3)] int? D { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] Element? B { get; set; }
    [PropertyOrder(4)] double X { get; set; }
}

interface Element
{
}

interface InheritedBaseWithoutCustomOrder
{
    string? BaseA { get; set; }
    string? BaseZ { get; set; }
    string? BaseB { get; set; }
}

interface InheritedOrderSubClassWithoutBaseOrder : InheritedBaseWithoutCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

interface InheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? BaseA { get; set; }
    [PropertyOrder(1)] string? BaseZ { get; set; }
    [PropertyOrder(2)] string? BaseB { get; set; }
}

interface InheritedOrderSubClassWithBaseOrder : InheritedBaseWithCustomOrder
{
    [PropertyOrder(0)] string? A { get; set; }
    [PropertyOrder(1)] string? Z { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}

interface InheritedOrderSubClassWithRedefinedBaseOrder : InheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? Z { get; set; }
    [PropertyOrder(1)] new string? B { get; set; }
    [PropertyOrder(2)] new string? A { get; set; }
}

// redeclare property order unchanged -> compile-only test
interface InheritedOrderSubClassWithRedefinedBaseOrderUnchanged : InheritedOrderSubClassWithBaseOrder
{
    [PropertyOrder(0)] new string? A { get; set; }
    [PropertyOrder(1)] new string? Z { get; set; }
    [PropertyOrder(2)] new string? B { get; set; }
}

[InterfaceOnly]
interface BaseClass
{
    [PropertyOrder(0)]
    [GetterOnly]
    object? Value { get; }
}

interface Inherited : BaseClass
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
