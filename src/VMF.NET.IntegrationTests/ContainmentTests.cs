using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests containment relationships: parent tracking, auto-unregister on move.
/// </summary>
public class ContainmentTests
{
    [Fact]
    public void AddChild_SetsContainer()
    {
        var flow = IFlow.NewInstance();
        var node = INode.NewInstance();
        node.Name = "N1";

        flow.Nodes.Add(node);

        Assert.Same(flow, node.Flow);
        Assert.Single(flow.Nodes);
        Assert.Same(node, flow.Nodes[0]);
    }

    [Fact]
    public void RemoveChild_ClearsContainer()
    {
        var flow = IFlow.NewInstance();
        var node = INode.NewInstance();

        flow.Nodes.Add(node);
        Assert.Same(flow, node.Flow);

        flow.Nodes.Remove(node);
        Assert.Null(node.Flow);
        Assert.Empty(flow.Nodes);
    }

    [Fact]
    public void MoveChild_BetweenParents()
    {
        var flow1 = IFlow.NewInstance();
        flow1.Title = "Flow1";
        var flow2 = IFlow.NewInstance();
        flow2.Title = "Flow2";

        var node = INode.NewInstance();
        node.Name = "Moveable";

        flow1.Nodes.Add(node);
        Assert.Same(flow1, node.Flow);
        Assert.Single(flow1.Nodes);

        // Adding to flow2 should auto-remove from flow1
        flow2.Nodes.Add(node);
        Assert.Same(flow2, node.Flow);
        Assert.Empty(flow1.Nodes);
        Assert.Single(flow2.Nodes);
    }

    [Fact]
    public void Connection_HasFlowContainer()
    {
        var flow = IFlow.NewInstance();
        var conn = IConnection.NewInstance();

        flow.Connections.Add(conn);
        Assert.Same(flow, conn.Flow);
    }

    [Fact]
    public void MultipleChildren_InOneParent()
    {
        var flow = IFlow.NewInstance();
        var n1 = INode.NewInstance();
        n1.Name = "A";
        var n2 = INode.NewInstance();
        n2.Name = "B";
        var n3 = INode.NewInstance();
        n3.Name = "C";

        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);
        flow.Nodes.Add(n3);

        Assert.Equal(3, flow.Nodes.Count);
        Assert.All(flow.Nodes, n => Assert.Same(flow, n.Flow));
    }
}
