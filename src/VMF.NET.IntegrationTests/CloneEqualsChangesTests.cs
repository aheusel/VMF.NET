using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests clone, content-based equals, toString, and change tracking.
/// </summary>
public class CloneEqualsChangesTests
{
    [Fact]
    public void Clone_DeepCopiesGraph()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "Original";
        var n1 = INode.NewInstance();
        n1.Name = "N1";
        n1.X = 10;
        flow.Nodes.Add(n1);

        var clone = flow.Clone();

        Assert.NotSame(flow, clone);
        Assert.Equal("Original", clone.Title);
        Assert.Single(clone.Nodes);
        Assert.NotSame(n1, clone.Nodes[0]);
        Assert.Equal("N1", clone.Nodes[0].Name);
        Assert.Equal(10, clone.Nodes[0].X);
        // Cloned node's container should be the cloned flow
        Assert.Same(clone, clone.Nodes[0].Flow);
    }

    [Fact]
    public void Clone_IndependentOfOriginal()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "V1";
        var node = INode.NewInstance();
        node.Name = "A";
        flow.Nodes.Add(node);

        var clone = flow.Clone();
        clone.Title = "V2";
        clone.Nodes[0].Name = "B";

        Assert.Equal("V1", flow.Title);
        Assert.Equal("A", flow.Nodes[0].Name);
    }

    [Fact]
    public void ContentEquals_SameContent_ReturnsTrue()
    {
        var f1 = IFlow.NewInstance();
        f1.Title = "Test";
        var n1 = INode.NewInstance();
        n1.Name = "A";
        n1.X = 5;
        f1.Nodes.Add(n1);

        var f2 = f1.Clone();

        Assert.Equal(f1, f2);
        Assert.Equal(f1.GetHashCode(), f2.GetHashCode());
    }

    [Fact]
    public void ContentEquals_DifferentContent_ReturnsFalse()
    {
        var f1 = IFlow.NewInstance();
        f1.Title = "A";

        var f2 = IFlow.NewInstance();
        f2.Title = "B";

        Assert.NotEqual(f1, f2);
    }

    [Fact]
    public void ToString_ContainsPropertyValues()
    {
        var node = INode.NewInstance();
        node.Name = "TestNode";
        node.X = 42;

        var str = node.ToString();
        Assert.Contains("TestNode", str);
        Assert.Contains("42", str);
    }

    [Fact]
    public void Changes_ListenerFires_OnPropertyChange()
    {
        var node = INode.NewInstance();
        var changes = new List<IChange>();

        node.Vmf().Changes().AddListener(c => changes.Add(c));
        node.Name = "Hello";

        Assert.Single(changes);
        Assert.Equal("Name", changes[0].PropertyName);
        Assert.NotNull(changes[0].PropertyChange);
        Assert.Null(changes[0].PropertyChange!.OldValue);
        Assert.Equal("Hello", changes[0].PropertyChange!.NewValue);
    }

    [Fact]
    public void Changes_ListenerFires_OnListChange()
    {
        var flow = IFlow.NewInstance();
        var changes = new List<IChange>();

        flow.Vmf().Changes().AddListener(c => changes.Add(c));
        var node = INode.NewInstance();
        flow.Nodes.Add(node);

        Assert.True(changes.Count >= 1);
        Assert.Contains(changes, c => c.PropertyName == "Nodes");
    }

    [Fact]
    public void Changes_Recording()
    {
        var flow = IFlow.NewInstance();
        var ch = flow.Vmf().Changes();
        ch.Start();

        flow.Title = "A";
        flow.Title = "B";

        Assert.Equal(2, ch.All().Count);
        ch.Stop();
    }
}
