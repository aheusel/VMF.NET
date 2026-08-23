// Ported from eu.mihosoft.vmftest.complex.unparsermodel.UnparserModelTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel;

public class UnparserModelTest
{
    [Fact]
    public void ContainmentWithInheritanceTest1()
    {
        // containment across an inheritance hierarchy: the property id depends on the runtime
        // type, not the statically declared one
        var alternative = IAlternative.NewInstance();
        var sre = IUPSubRuleElement.NewInstance();
        alternative.Elements.Add(sre);

        Assert.NotNull(sre.ParentAlt);
        Assert.Same(alternative, sre.ParentAlt);
    }

    [Fact]
    public void ContainmentWithInheritanceTest1_FromChildSide()
    {
        // the same containment, driven from the child instead of the parent
        var alternative = IAlternative.NewInstance();
        var sre = IUPSubRuleElement.NewInstance();

        sre.ParentAlt = alternative;

        Assert.Same(alternative, sre.ParentAlt);
        Assert.Contains(sre, alternative.Elements);

        // and detaching from the child side removes it from the parent's list
        sre.ParentAlt = null;

        Assert.Null(sre.ParentAlt);
        Assert.DoesNotContain(sre, alternative.Elements);
    }

    [Fact(Skip = "Needs VListChangeEvent.Source (Java evt.source()) so a change listener can " +
                 "mutate the list it is observing, plus AddRange/addAll raising a single event. " +
                 "VListChangeEvent exposes Added/Removed/Index but not the source list.")]
    public void TestRemoveDuringAddEventTest()
    {
    }
}
