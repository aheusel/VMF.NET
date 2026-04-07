using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests for shallow copy and annotation features.
/// </summary>
public class ShallowCopyAnnotationTests
{
    // --- Shallow Copy ---

    [Fact]
    public void ShallowCopy_CopiesScalarProperties()
    {
        var node = INode.NewInstance();
        node.Name = "Original";
        node.X = 10;
        node.Y = 20;

        var copy = node.Vmf().Content().ShallowCopy<INode>();

        Assert.NotSame(node, copy);
        Assert.Equal("Original", copy.Name);
        Assert.Equal(10, copy.X);
        Assert.Equal(20, copy.Y);
    }

    [Fact]
    public void ShallowCopy_SharesModelTypeReferences()
    {
        var conn = IConnection.NewInstance();
        var sender = INode.NewInstance();
        sender.Name = "S";
        conn.Sender = sender;

        var copy = conn.Vmf().Content().ShallowCopy<IConnection>();

        // Shallow copy shares the same Sender reference
        Assert.Same(sender, copy.Sender);
    }

    [Fact]
    public void ShallowCopy_CopiesCollectionItems_ByReference()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "Test";
        var n1 = INode.NewInstance();
        n1.Name = "N1";
        var n2 = INode.NewInstance();
        n2.Name = "N2";
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var copy = flow.Vmf().Content().ShallowCopy<IFlow>();

        Assert.NotSame(flow, copy);
        Assert.Equal("Test", copy.Title);
        Assert.Equal(2, copy.Nodes.Count);
        // In shallow copy, collection items are the same references (moved to new parent)
        Assert.Equal("N1", copy.Nodes[0].Name);
        Assert.Equal("N2", copy.Nodes[1].Name);
    }

    [Fact]
    public void ShallowCopy_IsIndependentForScalars()
    {
        var node = INode.NewInstance();
        node.Name = "A";

        var copy = node.Vmf().Content().ShallowCopy<INode>();
        copy.Name = "B";

        Assert.Equal("A", node.Name);
        Assert.Equal("B", copy.Name);
    }

    [Fact]
    public void DeepCopy_ViaContent_Works()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "Original";
        var node = INode.NewInstance();
        node.Name = "N1";
        flow.Nodes.Add(node);

        var copy = flow.Vmf().Content().DeepCopy<IFlow>();

        Assert.NotSame(flow, copy);
        Assert.NotSame(node, copy.Nodes[0]);
        Assert.Equal("N1", copy.Nodes[0].Name);
    }

    // --- Property Annotations ---

    [Fact]
    public void PropertyAnnotations_ContainmentInfo_ReturnsCorrectValues()
    {
        var flow = IFlow.NewInstance();
        var intern = (IVObjectInternal)flow;

        // IFlow.Title — no containment
        var titleId = intern.GetPropertyIdByName("Title");
        var titleAnnotations = intern.GetPropertyAnnotationsById(titleId);
        Assert.Contains(titleAnnotations, a => a.Key == "vmf:property:containment-info" && a.Value == "none");

        // IFlow.Nodes — contains INode
        var nodesId = intern.GetPropertyIdByName("Nodes");
        var nodesAnnotations = intern.GetPropertyAnnotationsById(nodesId);
        Assert.Contains(nodesAnnotations, a => a.Key == "vmf:property:containment-info" && a.Value.StartsWith("contained"));
    }

    [Fact]
    public void PropertyAnnotations_ContainerProp_HasContainerAnnotation()
    {
        var node = INode.NewInstance();
        var intern = (IVObjectInternal)node;

        // INode.Flow is a container property
        var flowId = intern.GetPropertyIdByName("Flow");
        var flowAnnotations = intern.GetPropertyAnnotationsById(flowId);
        Assert.Contains(flowAnnotations, a => a.Key == "vmf:property:containment-info" && a.Value.StartsWith("container"));
    }

    [Fact]
    public void PropertyAnnotations_InvalidId_ReturnsEmpty()
    {
        var flow = IFlow.NewInstance();
        var intern = (IVObjectInternal)flow;
        var annotations = intern.GetPropertyAnnotationsById(999);
        Assert.Empty(annotations);
    }

    // --- Type Annotations ---

    [Fact]
    public void TypeAnnotations_ReturnsAnnotations()
    {
        var flow = IFlow.NewInstance();
        var intern = (IVObjectInternal)flow;
        var annotations = intern.GetAnnotations();
        // IFlow has no type-level annotations or markers, so just check it doesn't throw
        Assert.NotNull(annotations);
    }

    [Fact]
    public void TypeAnnotations_ReadOnlyDelegates()
    {
        var flow = IFlow.NewInstance();
        var ro = (IVObjectInternal)flow.AsReadOnly();
        var annotations = ro.GetAnnotations();
        Assert.NotNull(annotations);
    }

    [Fact]
    public void PropertyAnnotations_ReadOnlyDelegates()
    {
        var flow = IFlow.NewInstance();
        var ro = (IVObjectInternal)flow.AsReadOnly();
        var titleId = ro.GetPropertyIdByName("Title");
        var annotations = ro.GetPropertyAnnotationsById(titleId);
        Assert.Contains(annotations, a => a.Key == "vmf:property:containment-info");
    }
}
