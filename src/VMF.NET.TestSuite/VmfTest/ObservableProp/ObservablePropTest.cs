// Ported from eu.mihosoft.vmftest.observableprop.ObservablePropTest
//
// Four of the five facts are Skip-ped because they need runtime capabilities VMF.NET does
// not have yet. The skip reasons name the missing capability, so the skip count is the
// parity gap for this area rather than the facts quietly disappearing.

using System.Collections.Generic;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ObservableProp;

public class ObservablePropTest
{
    [Fact]
    public void ObserveSimplePropertyTest()
    {
        var observed = IObserveMyProperties.NewInstance();
        var nameProperty = observed.Vmf().Reflect().PropertyByName("Name");
        Assert.NotNull(nameProperty);

        var expectedValues = new List<string?> { "ABC", "123", "", null };
        var actualValues1 = new List<string?>();
        var actualValues2 = new List<string?>();

        observed.Vmf().Changes().AddListener(
            change => actualValues1.Add((string?)change.PropertyChange!.NewValue));

        nameProperty!.AddChangeListener(
            change => actualValues2.Add((string?)change.PropertyChange!.NewValue));

        foreach (var expected in expectedValues)
        {
            observed.Name = expected;
        }

        Assert.Equal(expectedValues, actualValues1);
        Assert.Equal(expectedValues, actualValues2);
    }

    [Fact(Skip = "Needs a batch list removal (Java's VList.removeAll(int...)): removing two " +
                 "indices must raise ONE change carrying both elements. VList only exposes " +
                 "RemoveAt, which raises one change per element.")]
    public void ObserveListPropertyTest()
    {
    }

    [Fact(Skip = "Needs change observation through a read-only view: " +
                 "ReadOnly*Impl.Vmf().Changes() throws InvalidOperationException, while Java " +
                 "allows listening on a read-only wrapper.")]
    public void ObserveSimplePropertyOfReadOnlyTest()
    {
    }

    [Fact(Skip = "Needs change observation through a read-only view (see above).")]
    public void ObserveListPropertyReadOnlyTest()
    {
    }

    [Fact(Skip = "Needs static type reflection (Java's Type.type().reflect()): a VmfProperty " +
                 "obtained without an instance must throw on get/set/isSet/unset/listen. " +
                 "VMF.NET has no static entry point to a model type's reflection.")]
    public void ThrowExceptionIfRuntimeMethodsAreUsedForStaticReflection()
    {
    }
}
