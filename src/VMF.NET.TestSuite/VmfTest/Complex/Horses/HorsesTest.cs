// Ported from eu.mihosoft.vmftest.complex.horses.HorsesTest
//
// Hamcrest translation used here and throughout the suite:
//   contains(a, b)      -> Assert.Equal(new[] { a, b }, list)   exactly these, in this order
//   hasItem(a)          -> Assert.Contains(a, list)             membership only
//   not(hasItem(a))     -> Assert.DoesNotContain(a, list)
// The Java assertion messages are kept as trailing comments, since xUnit's Assert.Equal takes
// no message. Java's println calls are dropped -- they produce output and assert nothing.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Horses;

public class HorsesTest
{
    [Fact]
    public void HorseTest()
    {
        var horse1 = IHorse.NewBuilder().WithName("Larissa").Build();
        var horse2 = IHorse.NewBuilder().WithName("Dynastie").Build();
        var horse3 = IHorse.NewBuilder().WithName("Mike").Build();

        var owner1 = IOwner.NewBuilder().WithName("Horst Müller").WithHorses(horse1).Build();
        var owner2 = IOwner.NewBuilder().WithName("Berta Schmidt").WithHorses(horse2, horse3).Build();

        var barn1 = IHorseBarn.NewBuilder().WithHorses(horse1, horse2).Build();
        var barn2 = IHorseBarn.NewBuilder().WithHorses(horse3).Build();

        var tournament1 = ITournament.NewBuilder()
            .WithName("Spring Tournament")
            .WithHorses(horse1, horse2, horse3)
            .Build();

        // Horses 1 and 2 must be contained by barn 1
        Assert.Equal(new[] { horse1, horse2 }, barn1.Horses);
        // Horse 3 must be contained by barn 2
        Assert.Equal(new[] { horse3 }, barn2.Horses);

        // Horse 1 must be owned by owner 1
        Assert.Same(owner1, horse1.Owner);
        // Horse 2 must be owned by owner 2
        Assert.Same(owner2, horse2.Owner);
        // Horse 3 must be owned by owner 2
        Assert.Same(owner2, horse3.Owner);

        // Tournament 1 contains all horses
        Assert.Equal(new[] { horse1, horse2, horse3 }, tournament1.Horses);

        // move horse 1 from barn 1 to barn 2
        barn2.Horses.Add(horse1);
        // Horse 1 must be removed from barn 1
        Assert.DoesNotContain(horse1, barn1.Horses);
        // Horse 1 must be contained by barn 2
        Assert.Contains(horse1, barn2.Horses);

        // owner 2 sells horse 3 to owner 1
        horse3.Owner = owner1;

        // Horse 3 must be owned by owner 1
        Assert.Contains(horse3, owner1.Horses);
        // Horse 3 must be removed from owner 2
        Assert.DoesNotContain(horse3, owner2.Horses);

        // now we attend a second tournament. but since tournaments are only references we can be
        // referenced by multiple tournament objects
        var tournament2 = ITournament.NewBuilder()
            .WithName("Summer Tournament")
            .WithHorses(horse1, horse2, horse3)
            .Build();

        // Tournament 1 contains all horses
        Assert.Contains(horse1, tournament1.Horses);
        Assert.Contains(horse2, tournament1.Horses);
        Assert.Contains(horse3, tournament1.Horses);
        // Tournament 2 contains all horses
        Assert.Contains(horse1, tournament2.Horses);
        Assert.Contains(horse2, tournament2.Horses);
        Assert.Contains(horse3, tournament2.Horses);

        // Horse 1 attends two tournaments
        Assert.Equal(new[] { tournament1, tournament2 }, horse1.Tournaments);
    }

    [Fact]
    public void CrossRefTestForLists()
    {
        var owner = IOwner.NewBuilder().WithName("Larry Smith").Build();
        var horse1 = IHorse.NewBuilder().WithName("Lady").Build();

        // adding a horse to the same list multiple times should still result
        // in only one reference to this horse being contained
        owner.Horses.Add(horse1);
        owner.Horses.Add(horse1);
        owner.Horses.Add(horse1);

        // The horses list should only contain one reference to a horse
        Assert.Single(owner.Horses);
        // Only one reference to horse 1 should be contained in the horses list
        Assert.Equal(new[] { horse1 }, owner.Horses);
    }
}
