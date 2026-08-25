// Ported from eu.mihosoft.vmftest.observableprop.ObservablePropTest

using System.Collections.Generic;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ObservableProp;

public class ObservablePropTest
{
    [Fact]
    public void ObserveSimplePropertyTest()
    {
        var observed = ObserveMyProperties.NewInstance();
        var nameProperty = observed.VMF.Reflect.PropertyByName("Name");
        Assert.NotNull(nameProperty);

        var expectedValues = new List<string?> { "ABC", "123", "", null };
        var actualValues1 = new List<string?>();
        var actualValues2 = new List<string?>();

        observed.VMF.Changes.AddListener(
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

    [Fact]
    public void ObserveListPropertyTest()
    {
        var observed = ObserveMyProperties.NewInstance();
        var values = observed.VMF.Reflect.PropertyByName("Values");
        Assert.NotNull(values);

        var actualValues1 = new List<IReadOnlyList<object?>>();
        var actualValues2 = new List<IReadOnlyList<object?>>();

        observed.VMF.Changes.AddListener(change => Record(change, actualValues1));
        values!.AddChangeListener(change => Record(change, actualValues2));

        // three modifications -> three changes, whatever their element counts
        observed.Values.AddRange([1, 2, 3]);
        observed.Values.RemoveAt(2);
        observed.Values.RemoveAll(0, 1);

        Assert.Equal(3, actualValues1.Count);
        Assert.Equal(3, actualValues2.Count);

        // added three, then removed one, then removed two -- each as a single change
        Assert.Equal(3, actualValues1[0].Count);
        Assert.Equal(3, actualValues2[0].Count);
        Assert.Single(actualValues1[1]);
        Assert.Single(actualValues2[1]);
        Assert.Equal(2, actualValues1[2].Count);
        Assert.Equal(2, actualValues2[2].Count);
    }

    private static void Record(IChange change, List<IReadOnlyList<object?>> into)
    {
        var listChange = change.ListChange!;
        if (listChange.WasAdded) into.Add(listChange.Added);
        else if (listChange.WasRemoved) into.Add(listChange.Removed);
    }

    [Fact]
    public void ObserveSimplePropertyOfReadOnlyTest()
    {
        // a read-only view cannot cause changes, but it can observe them: both the view's
        // Changes() and a property obtained through the view see writes made through the
        // mutable object
        var observed = ObserveMyProperties.NewInstance();
        ReadOnlyObserveMyProperties observedRO = observed.AsReadOnly();

        var nameProperty = observedRO.VMF.Reflect.PropertyByName("Name");
        Assert.NotNull(nameProperty);

        var expectedValues = new List<string?> { "ABC", "123", "", null };
        var actualValues1 = new List<string?>();
        var actualValues2 = new List<string?>();

        observedRO.VMF.Changes.AddListener(
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

    [Fact]
    public void ObserveListPropertyReadOnlyTest()
    {
        var observed = ObserveMyProperties.NewInstance();
        ReadOnlyObserveMyProperties observedRO = observed.AsReadOnly();

        var values = observedRO.VMF.Reflect.PropertyByName("Values");
        Assert.NotNull(values);

        var actualValues1 = new List<IReadOnlyList<object?>>();
        var actualValues2 = new List<IReadOnlyList<object?>>();

        observedRO.VMF.Changes.AddListener(change => Record(change, actualValues1));
        values!.AddChangeListener(change => Record(change, actualValues2));

        observed.Values.AddRange([1, 2, 3]);
        observed.Values.RemoveAt(2);
        observed.Values.RemoveAll(0, 1);

        Assert.Equal(3, actualValues1.Count);
        Assert.Equal(3, actualValues2.Count);

        Assert.Equal(3, actualValues1[0].Count);
        Assert.Equal(3, actualValues2[0].Count);
        Assert.Single(actualValues1[1]);
        Assert.Single(actualValues2[1]);
        Assert.Equal(2, actualValues1[2].Count);
        Assert.Equal(2, actualValues2[2].Count);
    }

    [Fact]
    public void ThrowExceptionIfRuntimeMethodsAreUsedForStaticReflection()
    {
        // DEVIATION: Java's static entry point is ObserveMyProperties.type(); C# cannot use that
        // name, because a model may declare a property called Type. See StaticReflectionTest.
        var nameProperty = ObserveMyProperties.ModelType().Reflect().PropertyByName("Name");

        Assert.NotNull(nameProperty);

        // none of these may be used on a property that is not associated with an object
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.Get());
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.Set(null));
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.GetDefault());
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.SetDefault(null));
        Assert.ThrowsAny<System.Exception>(() => _ = nameProperty!.IsSet);
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.Unset());
        Assert.ThrowsAny<System.Exception>(() => nameProperty!.AddChangeListener(c => { }));
    }
}
