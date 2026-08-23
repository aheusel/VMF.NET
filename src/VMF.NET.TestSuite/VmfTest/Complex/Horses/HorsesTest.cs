// Ported from eu.mihosoft.vmftest.complex.horses.HorsesTest

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

        var owner1 = IOwner.NewBuilder().WithName("Horst Mueller").WithHorses(horse1).Build();
        var owner2 = IOwner.NewBuilder().WithName("Berta Schmidt").WithHorses(horse2, horse3).Build();

        var barn1 = IHorseBarn.NewBuilder().WithHorses(horse1, horse2).Build();
        var barn2 = IHorseBarn.NewBuilder().WithHorses(horse3).Build();

        var tournament1 = ITournament.NewBuilder()
            .WithName("Spring Tournament")
            .WithHorses(horse1, horse2, horse3)
            .Build();

        Assert.Contains(horse1, barn1.Horses);
        Assert.Contains(horse2, barn1.Horses);
        Assert.Contains(horse3, barn2.Horses);

        Assert.Same(owner1, horse1.Owner);
        Assert.Same(owner2, horse2.Owner);
        Assert.Same(owner2, horse3.Owner);

        Assert.Contains(horse1, tournament1.Horses);
        Assert.Contains(horse2, tournament1.Horses);
        Assert.Contains(horse3, tournament1.Horses);

        // move horse 1 from barn 1 to barn 2 -- containment is unique
        barn2.Horses.Add(horse1);
        Assert.DoesNotContain(horse1, barn1.Horses);
        Assert.Contains(horse1, barn2.Horses);

        // owner 2 sells horse 3 to owner 1 -- the cross-reference moves with it
        horse3.Owner = owner1;
        Assert.Contains(horse3, owner1.Horses);
        Assert.DoesNotContain(horse3, owner2.Horses);
    }

    [Fact(Skip = "Cross-reference lists do not reject duplicates. Adding the same element three " +
                 "times leaves three entries; Java keeps one, because a cross-reference is a set " +
                 "of references rather than a bag. The generated code already guards the OPPOSITE " +
                 "side (if (!impl.X.Contains(this)) impl.X.Add(...)) but the list being added to " +
                 "accepts duplicates -- VList would need to know the property is a cross-ref.")]
    public void CrossRefTestForLists()
    {
        var owner = IOwner.NewBuilder().WithName("Larry Smith").Build();
        var horse1 = IHorse.NewBuilder().WithName("Lady").Build();

        // adding the same horse repeatedly must still leave exactly one reference
        owner.Horses.Add(horse1);
        owner.Horses.Add(horse1);
        owner.Horses.Add(horse1);

        Assert.Single(owner.Horses);
        Assert.Contains(horse1, owner.Horses);
    }
}
