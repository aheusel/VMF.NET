// Ported from eu.mihosoft.vmftest.complex.vflow.LargeFlowModelTest and VFlowGlobalListenerTest.
// Java's println calls are dropped throughout.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Complex.VFlow;

public class VFlowTest
{
    [Fact]
    public void CreateAndUndoTest()
    {
        var flow = IVFlow.NewInstance();
        flow.VMF.Changes.Start();

        WorkflowTest(flow, 8, 6);

        var numNodes = flow.VMF.Content.DescendantsAndSelf().OfType<VNode>().Count();
        var numObjects = flow.VMF.Content.DescendantsAndSelf().Count();

        // we expect a certain number of nodes
        Assert.Equal(19681, numNodes);
        // we expect a certain number of objects
        Assert.Equal(236161, numObjects);

        var changesToRevert = new List<VMF.NET.Runtime.IChange>(flow.VMF.Changes.All());
        changesToRevert.Reverse();

        // ... and undo all changes
        foreach (var ch in changesToRevert)
        {
            ch.Undo();
        }

        numNodes = flow.VMF.Content.DescendantsAndSelf().OfType<VNode>().Count();
        numObjects = flow.VMF.Content.DescendantsAndSelf().Count();

        // after undo, we expect exactly one node
        Assert.Equal(1, numNodes);
        // after undo, we expect exactly one object
        Assert.Equal(1, numObjects);
    }

    private static void WorkflowTest(IVFlow workflow, int depth, int width)
    {
        if (depth < 1)
        {
            return;
        }

        string[] connectionTypes = { "control", "data", "event" };

        for (int i = 0; i < width; i++)
        {
            VNode n;

            if (i % 2 == 0)
            {
                var subFlow = workflow.NewSubFlow(null!)!;
                n = subFlow;
                WorkflowTest(subFlow, depth - 1, width);
            }
            else
            {
                n = workflow.NewNode(null!)!;
            }

            n.Name = "Node id=" + n.Id;

            string type = connectionTypes[i % connectionTypes.Length];

            n.AddInput(type);
            n.AddInput("event");

            for (int j = 0; j < 3; j++)
            {
                n.AddInput(type);
            }

            n.AddOutput(type);
            n.AddOutput("event");
            n.AddOutput(type);

            for (int j = 0; j < 3; j++)
            {
                n.AddOutput(type);
            }

            n.Width = 300;
            n.Height = 200;

            n.X = (i % 5) * (n.Width + 30);
            n.Y = (i / 5) * (n.Height + 30);
        }
    }

    /// <summary>
    /// Verifies <a href="https://github.com/miho/VMF/issues/36">issue 36</a>.
    /// </summary>
    [Fact]
    public void TestGlobalListener()
    {
        var flow = IVFlow.NewInstance();

        int nodesEvtCounter = 0;
        int parentEvtCounter = 0;

        flow.VMF.Changes.AddListener(change =>
        {
            if (change.PropertyName == "Nodes") nodesEvtCounter++;
        });

        var n1 = VNode.NewBuilder().WithName("my-name 1").Build();
        var n2 = VNode.NewBuilder().WithName("my-name 2").Build();
        var n3 = VNode.NewBuilder().WithName("my-name 3").Build();

        n1.VMF.Changes.AddListener(change =>
        {
            if (change.PropertyName == "Parent") parentEvtCounter++;
        });

        n2.VMF.Changes.AddListener(change =>
        {
            if (change.PropertyName == "Parent") parentEvtCounter++;
        });

        n3.VMF.Changes.AddListener(change =>
        {
            if (change.PropertyName == "Parent") parentEvtCounter++;
        });

        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);
        flow.Nodes.Add(n3);

        Assert.Equal(3, nodesEvtCounter);
        Assert.Equal(3, parentEvtCounter);

        nodesEvtCounter = 0;

        flow.Nodes.Remove(n1);
        flow.Nodes.Remove(n2);
        flow.Nodes.Remove(n3);

        Assert.Equal(3, nodesEvtCounter);
    }
}
