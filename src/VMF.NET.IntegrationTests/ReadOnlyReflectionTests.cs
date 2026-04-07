using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests read-only wrappers, reflection API, and content iteration.
/// </summary>
public class ReadOnlyReflectionTests
{
    [Fact]
    public void AsReadOnly_ReturnsReadOnlyWrapper()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "Test";

        var ro = flow.AsReadOnly();

        Assert.NotNull(ro);
        Assert.Equal("Test", ro.Title);
    }

    [Fact]
    public void ReadOnly_CollectionsAreMapped()
    {
        var flow = IFlow.NewInstance();
        var node = INode.NewInstance();
        node.Name = "N1";
        flow.Nodes.Add(node);

        var ro = flow.AsReadOnly();

        Assert.Single(ro.Nodes);
        Assert.Equal("N1", ro.Nodes[0].Name);
    }

    [Fact]
    public void ReadOnly_EqualsMatchesMutable()
    {
        var f1 = IFlow.NewInstance();
        f1.Title = "Same";
        var f2 = IFlow.NewInstance();
        f2.Title = "Same";

        Assert.Equal(f1.AsReadOnly(), f2.AsReadOnly());
        Assert.Equal(f1.AsReadOnly().GetHashCode(), f2.AsReadOnly().GetHashCode());
    }

    [Fact]
    public void ReadOnly_SameWrapperReturned()
    {
        var flow = IFlow.NewInstance();
        var ro1 = flow.AsReadOnly();
        var ro2 = flow.AsReadOnly();

        Assert.Same(ro1, ro2);
    }

    [Fact]
    public void Reflect_Properties()
    {
        var flow = IFlow.NewInstance();
        var reflect = flow.Vmf().Reflect();

        var props = reflect.Properties();
        Assert.Contains(props, p => p.Name == "Title");
        Assert.Contains(props, p => p.Name == "Nodes");
        Assert.Contains(props, p => p.Name == "Connections");
    }

    [Fact]
    public void Reflect_PropertyValueById()
    {
        var node = INode.NewInstance();
        node.Name = "Test";
        node.X = 42;

        var intern = (IVObjectInternal)node;
        var nameId = intern.GetPropertyIdByName("Name");
        var xId = intern.GetPropertyIdByName("X");

        Assert.Equal("Test", intern.GetPropertyValueById(nameId));
        Assert.Equal(42, intern.GetPropertyValueById(xId));
    }

    [Fact]
    public void Reflect_Type()
    {
        var flow = IFlow.NewInstance();
        var reflect = flow.Vmf().Reflect();

        Assert.Contains("IFlow", reflect.Type().Name);
    }

    [Fact]
    public void Content_Stream_ReturnsContainedObjects()
    {
        var flow = IFlow.NewInstance();
        var n1 = INode.NewInstance();
        n1.Name = "A";
        var n2 = INode.NewInstance();
        n2.Name = "B";
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var content = flow.Vmf().Content();
        var all = content.Stream().ToList();

        // Should contain the flow itself and both nodes
        Assert.Contains(flow, all);
        Assert.Contains(n1, all);
        Assert.Contains(n2, all);
    }

    [Fact]
    public void Content_StreamTyped_FiltersCorrectly()
    {
        var flow = IFlow.NewInstance();
        var n1 = INode.NewInstance();
        var conn = IConnection.NewInstance();
        flow.Nodes.Add(n1);
        flow.Connections.Add(conn);

        var nodes = flow.Vmf().Content().Stream<INode>().ToList();
        Assert.Single(nodes);
        Assert.Same(n1, nodes[0]);
    }

    [Fact]
    public void Content_DeepCopy_CreatesIndependentCopy()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "Original";
        var node = INode.NewInstance();
        node.Name = "N";
        flow.Nodes.Add(node);

        var copy = flow.Vmf().Content().DeepCopy<IFlow>();

        Assert.NotSame(flow, copy);
        Assert.Equal("Original", copy.Title);
        Assert.Single(copy.Nodes);
        Assert.NotSame(node, copy.Nodes[0]);

        copy.Title = "Modified";
        Assert.Equal("Original", flow.Title);
    }

    [Fact]
    public void CrossRef_Sender_Receiver()
    {
        var flow = IFlow.NewInstance();
        var n1 = INode.NewInstance();
        n1.Name = "Sender";
        var n2 = INode.NewInstance();
        n2.Name = "Receiver";
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var conn = IConnection.NewInstance();
        flow.Connections.Add(conn);

        conn.Sender = n1;
        conn.Receiver = n2;

        // Cross-ref: node.Outputs and node.Inputs should be updated
        Assert.Contains(conn, n1.Outputs);
        Assert.Contains(conn, n2.Inputs);
    }

    [Fact]
    public void CrossRef_Unset_RemovesFromOpposite()
    {
        var flow = IFlow.NewInstance();
        var n1 = INode.NewInstance();
        var n2 = INode.NewInstance();
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var conn = IConnection.NewInstance();
        flow.Connections.Add(conn);

        conn.Sender = n1;
        Assert.Contains(conn, n1.Outputs);

        conn.Sender = null;
        Assert.Empty(n1.Outputs);
    }
}
