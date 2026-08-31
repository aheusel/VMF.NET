using VMF.NET.TestSuite.Models;
using Xunit;

namespace VMF.NET.TestSuite;

/// <summary>
/// A model may delegate ToString(), as Java's @DelegateTo does. Before 0.3.1 the generator
/// emitted its own ToString() alongside the delegating one and the generated file did not
/// compile (CS0111); Java suppresses its own block instead -- impl/to-string.vm, guarded by
/// ModelType.isToStringMethodDelegated().
/// </summary>
public class ToStringDelegationTests
{
    [Fact]
    public void DelegatedToString_IsUsed()
    {
        var item = Item.NewBuilder().WithId("my item 1").Build();

        Assert.Equal("item: my item 1", item.ToString());
    }

    [Fact]
    public void DelegatedToString_OverridesRatherThanHides()
    {
        // In Java `public String toString()` overrides on its own. In C# the generated method
        // needs `override`, or a base-typed reference silently gets object.ToString() instead.
        object boxed = Item.NewBuilder().WithId("x").Build();

        Assert.Equal("item: x", boxed.ToString());
        Assert.Equal("item: x", $"{boxed}");
    }

    [Fact]
    public void DelegatedToString_MayWalkTheModel()
    {
        var store = Store.NewBuilder().WithId("my store").Build();
        store.Items.Add(Item.NewBuilder().WithId("my item 1").Build());
        store.Items.Add(Item.NewBuilder().WithId("my item 2").Build());

        var text = store.ToString();

        Assert.StartsWith("> store: my store", text);
        Assert.Contains("-> item: my item 1", text);
        Assert.Contains("-> item: my item 2", text);

        // the generated structural form must not leak through
        Assert.DoesNotContain("@type", text);
    }

    [Fact]
    public void ParentPrintingADelegatingChild_UsesTheChildsCustomForm()
    {
        // Crate does not delegate, so it prints structurally -- but the child does, and Java's
        // __vmf_toString appends the delegated toString() for exactly this case.
        var crate = Crate.NewInstance();
        crate.Label = "c1";
        crate.Items.Add(Item.NewBuilder().WithId("my item 1").Build());

        var text = crate.ToString();

        Assert.Contains("@type", text);              // the parent is structural
        Assert.Contains("item: my item 1", text);    // the child is not
    }
}
