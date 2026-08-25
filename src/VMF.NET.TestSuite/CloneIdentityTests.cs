// Regression tests for the clone identity map.
//
// Clone tracks which objects it has already copied so a graph reached twice yields one copy
// rather than two. That map was a plain Dictionary<object, object>, which compares keys with
// Equals -- so for a model using content equality, two DISTINCT but content-equal objects were
// treated as the same key and collapsed into a single copy. The clone then had fewer objects
// than the original and shared them where the original did not.

using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.VmfTest.Complex.Fsm;
using Xunit;

namespace VMF.NET.TestSuite;

public class CloneIdentityTests
{
    private static FSM BuildFsm(int transitions)
    {
        var fsm = FSM.NewInstance();
        for (int i = 0; i < transitions; i++)
        {
            var s = State.NewInstance();
            s.Name = "State " + i;
            fsm.OwnedState.Add(s);

            if (i > 0)
            {
                var t = Transition.NewInstance();
                var a = IAction.NewInstance();
                // every action carries the SAME name, so all of them are content-equal
                a.Name = "action";
                t.Actions.Add(a);

                var sender = fsm.OwnedState[i - 1];
                t.Input = sender.Name;
                t.Output = s.Name;
                sender.OutgoingTransitions.Add(t);
                s.IncomingTransitions.Add(t);
            }
        }
        return fsm;
    }

    private static int DistinctActions(FSM fsm) =>
        fsm.OwnedState
            .SelectMany(s => s.OutgoingTransitions.Concat(s.IncomingTransitions))
            .SelectMany(t => t.Actions)
            .Distinct(ReferenceEqualityComparer.Instance)
            .Count();

    [Fact]
    public void Clone_ContentEqualButDistinctObjects_StayDistinct()
    {
        var fsm = BuildFsm(4);
        var clone = fsm.Clone();

        Assert.Equal(3, DistinctActions(fsm));
        // the clone had 1 here: three transitions sharing one action
        Assert.Equal(DistinctActions(fsm), DistinctActions(clone));
    }

    [Fact]
    public void DeepCopy_ContentEqualButDistinctObjects_StayDistinct()
    {
        var fsm = BuildFsm(4);
        var copy = fsm.VMF.Content.DeepCopy<FSM>();

        Assert.Equal(DistinctActions(fsm), DistinctActions(copy));
    }

    [Fact]
    public void Clone_SharedObject_StaysShared()
    {
        // the behaviour the map exists for: one object reached twice yields ONE copy
        var fsm = FSM.NewInstance();
        var s = State.NewInstance();
        s.Name = "s";
        fsm.OwnedState.Add(s);
        fsm.InitialState = s;
        fsm.FinalState.Add(s);

        var clone = fsm.Clone();

        Assert.Same(clone.OwnedState[0], clone.InitialState);
        Assert.Same(clone.OwnedState[0], clone.FinalState[0]);
    }
}
