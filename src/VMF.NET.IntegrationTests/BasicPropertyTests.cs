using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests basic property access, NewInstance, and Builder patterns on generated code.
/// </summary>
public class BasicPropertyTests
{
    [Fact]
    public void NewInstance_CreatesObject()
    {
        var flow = IFlow.NewInstance();
        Assert.NotNull(flow);
        Assert.Null(flow.Title);
    }

    [Fact]
    public void SetAndGetProperties()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "My Flow";
        Assert.Equal("My Flow", flow.Title);
    }

    [Fact]
    public void IntProperties_DefaultToZero()
    {
        var node = INode.NewInstance();
        Assert.Equal(0, node.X);
        Assert.Equal(0, node.Y);
    }

    [Fact]
    public void IntProperties_SetAndGet()
    {
        var node = INode.NewInstance();
        node.X = 42;
        node.Y = -10;
        Assert.Equal(42, node.X);
        Assert.Equal(-10, node.Y);
    }

    [Fact]
    public void CollectionProperties_InitiallyEmpty()
    {
        var flow = IFlow.NewInstance();
        Assert.NotNull(flow.Nodes);
        Assert.Empty(flow.Nodes);
        Assert.NotNull(flow.Connections);
        Assert.Empty(flow.Connections);
    }

    [Fact]
    public void Builder_BuildsObjectWithProperties()
    {
        var flow = IFlow.NewBuilder()
            .WithTitle("Built Flow")
            .Build();

        Assert.Equal("Built Flow", flow.Title);
    }

    [Fact]
    public void Builder_WithCollections()
    {
        var n1 = INode.NewInstance();
        n1.Name = "A";
        var n2 = INode.NewInstance();
        n2.Name = "B";

        var flow = IFlow.NewBuilder()
            .WithTitle("Graph")
            .WithNodes(n1, n2)
            .Build();

        Assert.Equal("Graph", flow.Title);
        Assert.Equal(2, flow.Nodes.Count);
        Assert.Equal("A", flow.Nodes[0].Name);
        Assert.Equal("B", flow.Nodes[1].Name);
    }

    [Fact]
    public void Builder_ApplyFrom_CopiesProperties()
    {
        var original = INode.NewInstance();
        original.Name = "Source";
        original.X = 100;
        original.Y = 200;

        var copy = INode.NewBuilder().ApplyFrom(original).Build();
        Assert.Equal("Source", copy.Name);
        Assert.Equal(100, copy.X);
        Assert.Equal(200, copy.Y);
    }

    [Fact]
    public void Builder_ApplyTo_UpdatesExisting()
    {
        var target = INode.NewInstance();
        target.Name = "Old";

        INode.NewBuilder()
            .WithName("New")
            .WithX(50)
            .ApplyTo(target);

        Assert.Equal("New", target.Name);
        Assert.Equal(50, target.X);
    }
}
