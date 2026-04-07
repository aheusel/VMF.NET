using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

public class VObjectsTests
{
    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var node = INode.NewInstance();
        Assert.True(VObjects.Equals(node, node));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        Assert.True(VObjects.Equals(null, null));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        var node = INode.NewInstance();
        Assert.False(VObjects.Equals(node, null));
        Assert.False(VObjects.Equals(null, node));
    }

    [Fact]
    public void Equals_DifferentInstances_SameContent_ReturnsTrue()
    {
        // Generated equals compares content, so two empty nodes with same defaults are equal
        var n1 = INode.NewInstance();
        var n2 = INode.NewInstance();
        Assert.True(VObjects.Equals(n1, n2));
    }

    [Fact]
    public void Equals_DifferentInstances_DifferentContent_ReturnsFalse()
    {
        var n1 = INode.NewInstance();
        n1.Name = "A";
        var n2 = INode.NewInstance();
        n2.Name = "B";
        Assert.False(VObjects.Equals(n1, n2));
    }

    [Fact]
    public void Equals_Collections_ElementWise()
    {
        var list1 = new VList<string> { "a", "b", "c" };
        var list2 = new VList<string> { "a", "b", "c" };
        Assert.True(VObjects.Equals(list1, list2));
    }

    [Fact]
    public void Equals_Collections_DifferentSize_ReturnsFalse()
    {
        var list1 = new VList<string> { "a", "b" };
        var list2 = new VList<string> { "a", "b", "c" };
        Assert.False(VObjects.Equals(list1, list2));
    }

    [Fact]
    public void Equals_Collections_DifferentElements_ReturnsFalse()
    {
        var list1 = new VList<string> { "a", "b" };
        var list2 = new VList<string> { "a", "x" };
        Assert.False(VObjects.Equals(list1, list2));
    }

    [Fact]
    public void Equals_Primitives_DelegatesToObjectEquals()
    {
        Assert.True(VObjects.Equals(42, 42));
        Assert.False(VObjects.Equals(42, 99));
        Assert.True(VObjects.Equals("hello", "hello"));
    }
}
