using VMF.NET.TestSuite.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite;

public class VObjectsTests
{
    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var node = Node.NewInstance();
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
        var node = Node.NewInstance();
        Assert.False(VObjects.Equals(node, null));
        Assert.False(VObjects.Equals(null, node));
    }

    [Fact]
    public void Equals_DifferentInstances_SameContent_ReturnsTrue()
    {
        // Generated equals compares content, so two empty nodes with same defaults are equal
        var n1 = Node.NewInstance();
        var n2 = Node.NewInstance();
        Assert.True(VObjects.Equals(n1, n2));
    }

    [Fact]
    public void Equals_DifferentInstances_DifferentContent_ReturnsFalse()
    {
        var n1 = Node.NewInstance();
        n1.Name = "A";
        var n2 = Node.NewInstance();
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

    // ------------------------------------------------------------------
    // The last user-visible members the audit (issue #2) found unasserted: the EventInfo label
    // a VList attaches to the change events it raises, and the listener opt-out.
    // ------------------------------------------------------------------

    [Fact]
    public void VList_AttachesItsEventInfoToTheChangesItRaises()
    {
        var list = new VList<string> { EventInfo = "containment:Parent.Children" };

        VListChangeEvent? seen = null;
        list.AddChangeListener(e => seen = e);

        list.Add("first");

        Assert.NotNull(seen);
        Assert.Equal("containment:Parent.Children", seen!.EventInfo);
    }

    [Fact]
    public void VList_CarriesEventInfoOnRemovalToo()
    {
        var list = new VList<string> { EventInfo = "label" };
        list.Add("x");

        VListChangeEvent? seen = null;
        list.AddChangeListener(e => seen = e);

        list.RemoveAt(0);

        Assert.NotNull(seen);
        Assert.Equal("label", seen!.EventInfo);
    }
}
