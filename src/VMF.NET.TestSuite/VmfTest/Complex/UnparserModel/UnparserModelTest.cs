// Ported from eu.mihosoft.vmftest.complex.unparsermodel.UnparserModelTest

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel;

public class UnparserModelTest
{
    // Java's containmentWithInheritanceTest1 is one @Test containing two independent blocks, each
    // in its own `{ }` scope with fresh objects. They port as two facts: the split matches the
    // scoping Java already had, and it stops a failure in the first half from masking the second.
    [Fact]
    public void ContainmentWithInheritanceTest1()
    {
        // containment across an inheritance hierarchy: the property id depends on the runtime
        // type, not the statically declared one
        var alternative = Alternative.NewInstance();
        var sre = UPSubRuleElement.NewInstance();
        alternative.Elements.Add(sre);

        Assert.NotNull(sre.ParentAlt);
        Assert.Same(alternative, sre.ParentAlt);
    }

    [Fact]
    public void ContainmentWithInheritanceTest1_FromChildSide()
    {
        // the same containment, driven from the child instead of the parent
        var alternative = Alternative.NewInstance();
        var sre = UPSubRuleElement.NewInstance();

        sre.ParentAlt = alternative;

        Assert.Same(alternative, sre.ParentAlt);

        // Elements should contain sre
        Assert.Contains(sre, alternative.Elements);

        var a1 = Alternative.NewInstance();
        sre.Alternatives.Add(a1);

        // Alternative a1 should have sre as parent
        Assert.Same(sre, a1.ParentRule);

        // and detaching from the child side removes it from the parent's list.
        // Not in the Java fact, which sets the container but never clears it.
        sre.ParentAlt = null;

        Assert.Null(sre.ParentAlt);
        Assert.DoesNotContain(sre, alternative.Elements);
    }

    [Fact]
    public void TestRemoveDuringAddEventTest()
    {
        var cls = RuleClass.NewBuilder().WithName("RC1").Build();

        var pa1 = Property.NewBuilder().WithName("pa").Build();
        var pa2 = Property.NewBuilder().WithName("pa").Build();

        cls.Properties.AddRange([pa1, pa2]);
        cls.Properties.AddRange([pa1, pa2]);

        cls.Properties.AddChangeListener(evt =>
        {
            // remove duplicate properties
            foreach (Property p1 in evt.Added)
            {
                foreach (Property p2 in new List<Property>(evt.Source!.Cast<Property>()))
                {
                    if (!ReferenceEquals(p1, p2) && p1.Name == p2.Name)
                    {
                        evt.Source!.Remove(p1);
                    }
                }
            }
        });

        // Properties
        Assert.Equal(new[] { pa1, pa2 }, cls.Properties);
    }
}
