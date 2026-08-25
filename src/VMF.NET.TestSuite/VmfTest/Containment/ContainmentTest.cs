// Ported from eu.mihosoft.vmftest.containment.ContainmentTest
//
// Containment is UNIQUE: an element belongs to exactly one container property at a time.
// Adding/setting it somewhere else must detach it from wherever it was before -- across
// different container types, across single- and list-valued properties, and whether or not
// the containment declares an opposite.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Containment;

public class ContainmentTest
{
    [Fact]
    public void ContainmentWithBuilderTest()
    {
        // check that containment works with builder (i.e. that builder calls containment methods)
        var ca = ContainerOne.NewBuilder().WithElement1(Element.NewInstance()).Build();
        var e = ca.Element1;

        Assert.Same(ca, e!.ParentOne);
    }

    [Fact]
    public void ContainmentTest_IsUnique()
    {
        // containment should be unique -- first check that containment works
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element1 = e;
        Assert.Same(ca, e.ParentOne);

        // if we set to a second container instance...
        var cb = ContainerOne.NewInstance();
        // ...should work like before and...
        cb.Element1 = e;
        Assert.Same(cb, e.ParentOne);

        // ...should unregister from previous container
        Assert.Null(ca.Element1);
    }

    [Fact]
    public void ContainmentMultiplePropsTest1()
    {
        // case 1: containments with opposites only
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element1 = e;

        var cb = ContainerTwo.NewInstance();
        cb.Element2 = e;

        Assert.Same(cb, e.ParentTwo);
        Assert.Null(ca.Element1);
    }

    [Fact]
    public void ContainmentMultiplePropsTest2()
    {
        // case 2: mixing containments with and without opposites
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element1 = e;
        Assert.Same(e, ca.Element1);

        var cb = ContainerTwo.NewInstance();
        cb.Element = e;
        Assert.Same(e, cb.Element);

        Assert.Null(ca.Element1);
    }

    [Fact]
    public void ContainmentMultiplePropsTest3()
    {
        // case 3: mixing containments with and without opposites (order swapped vs. case 1)
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element = e;
        Assert.Same(e, ca.Element);

        var cb = ContainerTwo.NewInstance();
        cb.Element2 = e;
        Assert.Same(cb, e.ParentTwo);

        Assert.Null(ca.Element);
    }

    [Fact]
    public void ContainmentMultiplePropsTest4()
    {
        // case 4: single without opposite, then element list WITH opposite
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element = e;
        Assert.Same(e, ca.Element);

        var cb = ContainerTwo.NewInstance();
        cb.Elements2a.Add(e);
        Assert.Contains(e, cb.Elements2a);

        Assert.Null(ca.Element);
    }

    [Fact]
    public void ContainmentMultiplePropsTest5()
    {
        // case 5: containments without opposites (single prop, then list)
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element = e;
        Assert.Same(e, ca.Element);

        var cb = ContainerTwo.NewInstance();
        cb.Elements2.Add(e);
        Assert.Contains(e, cb.Elements2);

        Assert.Null(ca.Element);
    }

    [Fact]
    public void ContainmentMultiplePropsTest6()
    {
        // case 6: one list without opposite, the other with
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Elements1.Add(e);
        Assert.Contains(e, ca.Elements1);

        var cb = ContainerTwo.NewInstance();
        cb.Elements2a.Add(e);
        Assert.Contains(e, cb.Elements2a);

        Assert.DoesNotContain(e, ca.Elements1);
    }

    [Fact]
    public void ContainmentMultiplePropsTest7()
    {
        // case 7: one list with opposite, the other without (reverse order of case 6)
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Elements1a.Add(e);
        Assert.Contains(e, ca.Elements1a);

        var cb = ContainerTwo.NewInstance();
        cb.Elements2.Add(e);

        Assert.DoesNotContain(e, ca.Elements1a);
        Assert.Contains(e, cb.Elements2);
    }

    [Fact]
    public void ContainmentMultiplePropsTestNoOpposite()
    {
        // containments without opposites only
        var ca = ContainerOne.NewInstance();
        var e = Element.NewInstance();
        ca.Element = e;
        Assert.Same(e, ca.Element);

        var cb = ContainerTwo.NewInstance();
        cb.Element = e;
        Assert.Same(e, cb.Element);

        Assert.Null(ca.Element);
    }

    [Fact]
    public void ContainmentMultiplePropsTestMultipleOpposites()
    {
        // containment must stay unique among multiple properties of the SAME container type,
        // where the element's [Container] has no single opposite

        // ONE-TO-ONE
        var ca = ContainerMultipleOpposites.NewInstance();
        var e = ElementMultipleOpposites.NewInstance();
        ca.Element = e;

        Assert.Same(e, ca.Element);
        Assert.Same(ca, e.Parent);

        var cb = ContainerMultipleOpposites.NewInstance();
        cb.Element = e;

        Assert.Null(ca.Element);
        Assert.Same(cb, e.Parent);

        // ONE-TO-MANY
        ca.Elements.Add(e);
        Assert.Null(cb.Element);
        Assert.Same(ca, e.Parent);

        // adding e to another list...
        ca.Elements1.Add(e);
        // ...should remove it from the previous list
        Assert.DoesNotContain(e, ca.Elements);
        Assert.Contains(e, ca.Elements1);

        // adding e to a single-valued property...
        ca.Element = e;
        // ...should remove it from the previous list
        Assert.DoesNotContain(e, ca.Elements1);
    }
}
