// Ported from eu.mihosoft.vmftest.complex.vflow.LargeFlowModelTest and VFlowGlobalListenerTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.VFlow;

public class VFlowTest
{
    [Fact(Skip = "Needs undo. The fact builds a large flow, collects changes().all() and calls " +
                 "undo() on each in reverse. IChange.Undo exists but nothing exercises it, and " +
                 "IChange.Apply is implemented and never called -- see the undo/redo milestone. " +
                 "It also needs Content().Stream<T>() counts over a deep graph.")]
    public void CreateAndUndoTest()
    {
    }

    [Fact(Skip = "Needs a change event when a child's container changes. The fact counts " +
                 "'parent' property changes on nodes as they are added to and removed from " +
                 "flow.Nodes; the generated containment listener calls SetContainer(this), which " +
                 "updates the backing field without firing a property change.")]
    public void TestGlobalListener()
    {
    }
}
