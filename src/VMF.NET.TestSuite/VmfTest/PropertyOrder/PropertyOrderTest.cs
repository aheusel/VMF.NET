// Ported from eu.mihosoft.vmftest.propertyorder.PropertyOrderTest
//
// Reflected properties are visited in a defined order: alphabetical by default, by
// [PropertyOrder] index when given, and with inherited properties always before own ones.

using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.PropertyOrder;

public class PropertyOrderTest
{
    private static string[] OrderOf(VMF.NET.Runtime.IVObject o) =>
        o.Vmf().Reflect().Properties().Select(p => p.Name).ToArray();

    [Fact]
    public void PropertyDefaultOrderTest()
    {
        // no custom order -> alphabetical
        Assert.Equal(new[] { "B", "D", "X", "Z" }, OrderOf(IDefaultOrder.NewInstance()));
    }

    [Fact]
    public void PropertyCustomOrderTest()
    {
        // [PropertyOrder] indices 1..4 over Z, B, D, X
        Assert.Equal(new[] { "Z", "B", "D", "X" }, OrderOf(ICustomOrder.NewInstance()));
    }

    [Fact]
    public void InheritedPropertyOrderTestWithoutBaseOrder()
    {
        // base has no custom order (so alphabetical), own properties do -- inherited first
        Assert.Equal(new[] { "BaseA", "BaseB", "BaseZ", "A", "Z", "B" },
                     OrderOf(IInheritedOrderSubClassWithoutBaseOrder.NewInstance()));
    }

    [Fact]
    public void InheritedPropertyOrderTestWithBaseOrder()
    {
        // base defines its own order, which the subtype must honour
        Assert.Equal(new[] { "BaseA", "BaseZ", "BaseB", "A", "Z", "B" },
                     OrderOf(IInheritedOrderSubClassWithBaseOrder.NewInstance()));
    }

    [Fact]
    public void InheritedPropertyOrderTestWithRedefinedBaseOrder()
    {
        // the subtype re-declares the order of its own properties
        Assert.Equal(new[] { "BaseA", "BaseZ", "BaseB", "Z", "B", "A" },
                     OrderOf(IInheritedOrderSubClassWithRedefinedBaseOrder.NewInstance()));
    }
}
