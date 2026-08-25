// Ported from eu.mihosoft.vmftest.cross_ref.CrossRefTest
//
// Setting one side of a cross-reference wires up the opposite. Both objects report a change
// EVENT, but the change is only RECORDED on the object the update was initiated on -- the
// opposite's update is an echo of the same logical change.

using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.CrossRef;

public class CrossRefTest
{
    private static Counter CountChangeEvents(IVObject o)
    {
        var counter = new Counter();
        o.VMF.Changes.AddListener(_ => counter.Value++);
        return counter;
    }

    private sealed class Counter
    {
        public int Value;
    }

    [Fact]
    public void SingleRefTest()
    {
        // one-to-one, initiated from the A side
        {
            var entityOneA = EntityOneA.NewInstance();
            var entityTwoA = EntityTwoA.NewInstance();
            var numEvtOneA = CountChangeEvents(entityOneA);
            var numEvtTwoA = CountChangeEvents(entityTwoA);
            entityOneA.VMF.Changes.Start();
            entityTwoA.VMF.Changes.Start();

            entityOneA.Ref = entityTwoA;

            Assert.Same(entityOneA, entityTwoA.Ref);
            Assert.Single(entityOneA.VMF.Changes.All());
            Assert.Equal(1, numEvtOneA.Value);
            Assert.Equal(1, numEvtTwoA.Value);
            // recorded only on the initiating object
            Assert.Empty(entityTwoA.VMF.Changes.All());
        }
        // ...and symmetrically, initiated from the B side
        {
            var entityOneA = EntityOneA.NewInstance();
            var entityTwoA = EntityTwoA.NewInstance();
            var numEvtOneA = CountChangeEvents(entityOneA);
            var numEvtTwoA = CountChangeEvents(entityTwoA);
            entityOneA.VMF.Changes.Start();
            entityTwoA.VMF.Changes.Start();

            entityTwoA.Ref = entityOneA;

            Assert.Same(entityTwoA, entityOneA.Ref);
            Assert.Single(entityTwoA.VMF.Changes.All());
            Assert.Equal(1, numEvtTwoA.Value);
            Assert.Equal(1, numEvtOneA.Value);
            Assert.Empty(entityOneA.VMF.Changes.All());
        }
    }

    [Fact]
    public void SingleMultipleRefTest()
    {
        // one-to-many, initiated from the list side
        {
            var entityOneB = EntityOneB.NewInstance();
            var entityTwoB = EntityTwoB.NewInstance();
            var numEvtOneB = CountChangeEvents(entityOneB);
            var numEvtTwoB = CountChangeEvents(entityTwoB);
            entityOneB.VMF.Changes.Start();
            entityTwoB.VMF.Changes.Start();

            entityTwoB.Refs.Add(entityOneB);

            Assert.Same(entityTwoB, entityOneB.Ref);
            Assert.Equal(1, numEvtOneB.Value);
            Assert.Equal(1, numEvtTwoB.Value);
            Assert.Single(entityTwoB.VMF.Changes.All());
            Assert.Empty(entityOneB.VMF.Changes.All());
        }
        // ...and from the single side
        {
            var entityOneB = EntityOneB.NewInstance();
            var entityTwoB = EntityTwoB.NewInstance();
            var numEvtOneB = CountChangeEvents(entityOneB);
            var numEvtTwoB = CountChangeEvents(entityTwoB);
            entityOneB.VMF.Changes.Start();
            entityTwoB.VMF.Changes.Start();

            entityOneB.Ref = entityTwoB;

            // opposite refs must contain ref (exactly, as Java's contains(...) asserts)
            Assert.Equal(new[] { entityOneB }, entityTwoB.Refs);
            Assert.Equal(1, numEvtOneB.Value);
            Assert.Equal(1, numEvtTwoB.Value);
            Assert.Empty(entityTwoB.VMF.Changes.All());
            Assert.Single(entityOneB.VMF.Changes.All());
        }
    }

    [Fact]
    public void MultipleMultipleRefTest()
    {
        // many-to-many, initiated from either side
        {
            var entityOneC = EntityOneC.NewInstance();
            var entityTwoC = EntityTwoC.NewInstance();
            var numEvtOneC = CountChangeEvents(entityOneC);
            var numEvtTwoC = CountChangeEvents(entityTwoC);
            entityOneC.VMF.Changes.Start();
            entityTwoC.VMF.Changes.Start();

            entityOneC.Refs.Add(entityTwoC);

            // opposite refs must contain ref (exactly)
            Assert.Equal(new[] { entityOneC }, entityTwoC.Refs);
            Assert.Equal(1, numEvtOneC.Value);
            Assert.Equal(1, numEvtTwoC.Value);
            Assert.Empty(entityTwoC.VMF.Changes.All());
            Assert.Single(entityOneC.VMF.Changes.All());
        }
        {
            var entityOneC = EntityOneC.NewInstance();
            var entityTwoC = EntityTwoC.NewInstance();
            var numEvtOneC = CountChangeEvents(entityOneC);
            var numEvtTwoC = CountChangeEvents(entityTwoC);
            entityOneC.VMF.Changes.Start();
            entityTwoC.VMF.Changes.Start();

            entityTwoC.Refs.Add(entityOneC);

            // opposite refs must contain ref (exactly)
            Assert.Equal(new[] { entityTwoC }, entityOneC.Refs);
            Assert.Equal(1, numEvtOneC.Value);
            Assert.Equal(1, numEvtTwoC.Value);
            Assert.Empty(entityOneC.VMF.Changes.All());
            Assert.Single(entityTwoC.VMF.Changes.All());
        }
    }

    // --- regression cover for the wiring itself (not from the Java suite) ---
    //
    // A bidirectional single-valued cross-reference used to recurse until the stack overflowed:
    // the setter assigned its field only AFTER syncing the opposite, so when the opposite set
    // this side back, the ReferenceEquals guard still saw the old value.

    [Fact]
    public void SingleRef_SetsOppositeWithoutRecursing()
    {
        var one = EntityOneA.NewInstance();
        var two = EntityTwoA.NewInstance();

        one.Ref = two;

        Assert.Same(two, one.Ref);
        Assert.Same(one, two.Ref);
    }

    [Fact]
    public void SingleRef_SetsOppositeWhenAssignedFromTheOtherSide()
    {
        var one = EntityOneA.NewInstance();
        var two = EntityTwoA.NewInstance();

        two.Ref = one;

        Assert.Same(one, two.Ref);
        Assert.Same(two, one.Ref);
    }

    [Fact]
    public void SingleToMany_SetsBothSides()
    {
        var one = EntityOneB.NewInstance();
        var two = EntityTwoB.NewInstance();

        two.Refs.Add(one);
        Assert.Same(two, one.Ref);
        Assert.Contains(one, two.Refs);

        var one2 = EntityOneB.NewInstance();
        var two2 = EntityTwoB.NewInstance();
        one2.Ref = two2;
        Assert.Contains(one2, two2.Refs);
        Assert.Same(two2, one2.Ref);
    }

    [Fact]
    public void ManyToMany_SetsBothSides()
    {
        var one = EntityOneC.NewInstance();
        var two = EntityTwoC.NewInstance();

        one.Refs.Add(two);

        Assert.Contains(two, one.Refs);
        Assert.Contains(one, two.Refs);
    }
}
