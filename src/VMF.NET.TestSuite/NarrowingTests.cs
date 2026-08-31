using System;
using VMF.NET.TestSuite.Models;
using Xunit;

namespace VMF.NET.TestSuite;

/// <summary>
/// Covariant property narrowing. Java narrows by overriding a getter with a narrower return
/// type; C# has no covariant override for an interface property, so the model re-declares with
/// `new` and the generated implementation carries the member at the narrowed type, satisfying
/// each wider declaration with a forwarding explicit implementation.
///
/// Reading behaves as Java does. The two behavioural differences are pinned below.
/// </summary>
public class NarrowingTests
{
    [Fact]
    public void NarrowedProperty_IsVisibleAtBothTypes()
    {
        var holder = RoundHolder.NewInstance();
        var round = Round.NewInstance();
        round.Label = "r";

        holder.Value = round;

        Assert.Equal("r", holder.Value!.Label);                    // narrow view
        Assert.Same(round, ((GlyphHolder)holder).Value);           // wide view, same object
    }

    [Fact]
    public void NarrowedSetter_RejectsANonFittingValueAtTheAssignment()
    {
        // DIFFERENCE TO JAVA, measured 2026-08-30. Java stores the value and throws at the next
        // narrowed read; VMF.NET throws here, at the assignment. Both reject it -- the failure
        // just surfaces at a different point, and VMF.NET's is the earlier of the two.
        var holder = RoundHolder.NewInstance();
        var boxy = Boxy.NewInstance();

        Assert.Throws<InvalidCastException>(() => ((GlyphHolder)holder).Value = boxy);

        // and nothing was stored
        Assert.Null(holder.Value);
    }
}
