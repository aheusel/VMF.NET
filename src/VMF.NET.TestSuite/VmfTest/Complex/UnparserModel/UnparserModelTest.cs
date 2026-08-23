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

    [Fact(Skip = "Needs a settable [Container] property. The Java fact drives containment from " +
                 "the child side (sre.setParentAlt(alternative)); VMF.NET never generates a " +
                 "setter for a [Container] property, so only the parent side can drive it.")]
    public void ContainmentWithInheritanceTest1_FromChildSide()
    {
    }

    [Fact(Skip = "Needs VListChangeEvent.Source (Java evt.source()) so a change listener can " +
                 "mutate the list it is observing, plus AddRange/addAll raising a single event. " +
                 "VListChangeEvent exposes Added/Removed/Index but not the source list.")]
    public void TestRemoveDuringAddEventTest()
    {
    }
}
