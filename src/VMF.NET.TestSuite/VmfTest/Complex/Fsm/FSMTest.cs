// Ported from eu.mihosoft.vmftest.complex.fsm.FSMTest
//
// Builds a large state machine, clones it, and requires the clone to be equal to the original
// by content and by ToString().
//
// Two things in the Java original are deliberately absent:
//
//  - the timing printouts, and
//  - the two `for (int j = 0; j < numMeasurements; j++)` loops around graph construction and
//    cloning. numMeasurements is 1, so each loop body runs exactly once; they exist only to let
//    someone average the timings by raising that constant. Porting them would add no assertion
//    and no coverage.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.Fsm;

public class FSMTest
{
    private const int NumTransitions = 10_000;

    private static FSM BuildFsm(int numTransitions)
    {
        var fsm = FSM.NewInstance();
        for (int i = 0; i < numTransitions; i++)
        {
            var s = State.NewInstance();
            s.Name = "State " + i;
            fsm.OwnedState.Add(s);

            if (i > 0)
            {
                var transition = Transition.NewInstance();
                var a = IAction.NewInstance();
                a.Name = "action";
                transition.Actions.Add(a);

                var sender = fsm.OwnedState[i - 1];
                var receiver = s;
                transition.Input = sender.Name;
                transition.Output = receiver.Name;
                sender.OutgoingTransitions.Add(transition);
                receiver.IncomingTransitions.Add(transition);
            }
        }

        fsm.InitialState = fsm.OwnedState[0];
        fsm.FinalState.Add(fsm.OwnedState[fsm.OwnedState.Count - 1]);

        return fsm;
    }

    [Fact]
    public void FsmCreateAndCloneTest()
    {
        var fsm = BuildFsm(NumTransitions);

        var clone = fsm.Clone();

        Assert.Equal(fsm.OwnedState.Count, clone.OwnedState.Count);
        Assert.Equal(NumTransitions, fsm.OwnedState.Count);
        Assert.Equal(fsm, clone);
        Assert.Equal(fsm.ToString(), clone.ToString());
    }
}
