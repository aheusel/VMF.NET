// Ported from eu.mihosoft.vmftest.complex.unparsermodel.UnparserModelTest

using System.Collections.Generic;
using System.Linq;
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

        // Elements should contain sre
        Assert.Contains(sre, alternative.Elements);

        var a1 = IAlternative.NewInstance();
        sre.Alternatives.Add(a1);

        // Alternative a1 should have sre as parent
        Assert.Same(sre, a1.ParentRule);

        // and detaching from the child side removes it from the parent's list
        // (not in the Java fact -- it has no way to drive containment from the child)
        sre.ParentAlt = null;

        Assert.Null(sre.ParentAlt);
        Assert.DoesNotContain(sre, alternative.Elements);
    }

    [Fact]
    public void TestRemoveDuringAddEventTest()
    {
        var cls = IRuleClass.NewBuilder().WithName("RC1").Build();

        var pa1 = IProperty.NewBuilder().WithName("pa").Build();
        var pa2 = IProperty.NewBuilder().WithName("pa").Build();

        cls.Properties.AddRange([pa1, pa2]);
        cls.Properties.AddRange([pa1, pa2]);

        cls.Properties.AddChangeListener(evt =>
        {
            // remove duplicate properties
            foreach (IProperty p1 in evt.Added)
            {
                foreach (IProperty p2 in new List<IProperty>(evt.Source!.Cast<IProperty>()))
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
