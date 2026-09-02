using VMF.NET.TestSuite.Models;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;
using Xunit;

namespace VMF.NET.TestSuite;

/// <summary>
/// Tests read-only wrappers, reflection API, and content iteration.
/// </summary>
public class ReadOnlyReflectionTests
{
    [Fact]
    public void AsReadOnly_ReturnsReadOnlyWrapper()
    {
        var flow = Flow.NewInstance();
        flow.Title = "Test";

        var ro = flow.AsReadOnly();

        Assert.NotNull(ro);
        Assert.Equal("Test", ro.Title);
    }

    [Fact]
    public void ReadOnly_CollectionsAreMapped()
    {
        var flow = Flow.NewInstance();
        var node = Node.NewInstance();
        node.Name = "N1";
        flow.Nodes.Add(node);

        var ro = flow.AsReadOnly();

        Assert.Single(ro.Nodes);
        Assert.Equal("N1", ro.Nodes[0].Name);
    }

    [Fact]
    public void ReadOnly_EqualsMatchesMutable()
    {
        var f1 = Flow.NewInstance();
        f1.Title = "Same";
        var f2 = Flow.NewInstance();
        f2.Title = "Same";

        Assert.Equal(f1.AsReadOnly(), f2.AsReadOnly());
        Assert.Equal(f1.AsReadOnly().GetHashCode(), f2.AsReadOnly().GetHashCode());
    }

    [Fact]
    public void ReadOnly_SameWrapperReturned()
    {
        var flow = Flow.NewInstance();
        var ro1 = flow.AsReadOnly();
        var ro2 = flow.AsReadOnly();

        Assert.Same(ro1, ro2);
    }

    [Fact]
    public void Reflect_Properties()
    {
        var flow = Flow.NewInstance();
        var reflect = flow.VMF.Reflect;

        var props = reflect.Properties();
        Assert.Contains(props, p => p.Name == "Title");
        Assert.Contains(props, p => p.Name == "Nodes");
        Assert.Contains(props, p => p.Name == "Connections");
    }

    [Fact]
    public void Reflect_PropertyValueById()
    {
        var node = Node.NewInstance();
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
        var flow = Flow.NewInstance();
        var reflect = flow.VMF.Reflect;

        Assert.Contains("Flow", reflect.Type().Name);
    }

    [Fact]
    public void Content_Stream_ReturnsContainedObjects()
    {
        var flow = Flow.NewInstance();
        var n1 = Node.NewInstance();
        n1.Name = "A";
        var n2 = Node.NewInstance();
        n2.Name = "B";
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var content = flow.VMF.Content;
        var all = content.Traverse().ToList();

        // Should contain the flow itself and both nodes
        Assert.Contains(flow, all);
        Assert.Contains(n1, all);
        Assert.Contains(n2, all);
    }

    [Fact]
    public void Content_StreamTyped_FiltersCorrectly()
    {
        var flow = Flow.NewInstance();
        var n1 = Node.NewInstance();
        var conn = Connection.NewInstance();
        flow.Nodes.Add(n1);
        flow.Connections.Add(conn);

        var nodes = flow.VMF.Content.Traverse().OfType<Node>().ToList();
        Assert.Single(nodes);
        Assert.Same(n1, nodes[0]);
    }

    [Fact]
    public void Content_DeepCopy_CreatesIndependentCopy()
    {
        var flow = Flow.NewInstance();
        flow.Title = "Original";
        var node = Node.NewInstance();
        node.Name = "N";
        flow.Nodes.Add(node);

        var copy = flow.VMF.Content.DeepCopy<Flow>();

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
        var flow = Flow.NewInstance();
        var n1 = Node.NewInstance();
        n1.Name = "Sender";
        var n2 = Node.NewInstance();
        n2.Name = "Receiver";
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var conn = Connection.NewInstance();
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
        var flow = Flow.NewInstance();
        var n1 = Node.NewInstance();
        var n2 = Node.NewInstance();
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var conn = Connection.NewInstance();
        flow.Connections.Add(conn);

        conn.Sender = n1;
        Assert.Contains(conn, n1.Outputs);

        conn.Sender = null;
        Assert.Empty(n1.Outputs);
    }

    // ------------------------------------------------------------------
    // AsModifiable -- Java's ReadOnly.asModifiable()
    //
    // Java: read-only-implementation.vm:198 returns `this.mutableObject.clone()`, and
    // clone() (impl/clone.vm) is _vmf_deepCopy over an IdentityHashMap. So it is a full
    // deep copy, never an alias to the wrapped object -- which is what makes handing out
    // a read-only view safe. These pin exactly that.
    //
    // No Java test covers asModifiable(), which is why VMF.NET shipped 0.3.0 without the
    // method at all; the ported suite had nothing to notice its absence.
    // ------------------------------------------------------------------

    [Fact]
    public void AsModifiable_ReturnsAWritableCopy()
    {
        var flow = Flow.NewInstance();
        flow.Title = "Original";

        Flow copy = flow.AsReadOnly().AsModifiable();

        Assert.NotSame(flow, copy);
        Assert.Equal("Original", copy.Title);

        // it is genuinely modifiable -- the entire point of the method
        copy.Title = "Changed";
        Assert.Equal("Changed", copy.Title);
        Assert.Equal("Original", flow.Title);
    }

    [Fact]
    public void AsModifiable_CopiesContainedChildrenDeeply()
    {
        var flow = Flow.NewInstance();
        var node = Node.NewInstance();
        node.Name = "N1";
        flow.Nodes.Add(node);

        var copy = flow.AsReadOnly().AsModifiable();

        Assert.Single(copy.Nodes);
        Assert.NotSame(node, copy.Nodes[0]);

        copy.Nodes[0].Name = "changed";
        Assert.Equal("N1", node.Name);
    }

    [Fact]
    public void AsModifiable_IsASnapshotWhereAsReadOnlyIsALiveView()
    {
        var flow = Flow.NewInstance();
        flow.Title = "Before";

        var view = flow.AsReadOnly();
        var copy = view.AsModifiable();

        flow.Title = "After";

        Assert.Equal("After", view.Title);   // the view tracks the original
        Assert.Equal("Before", copy.Title);  // the copy does not
    }

    [Fact]
    public void AsModifiable_PreservesSharedReferences()
    {
        var flow = Flow.NewInstance();
        var sender = Node.NewInstance();
        sender.Name = "sender";
        var receiver = Node.NewInstance();
        receiver.Name = "receiver";
        flow.Nodes.Add(sender);
        flow.Nodes.Add(receiver);

        var conn = Connection.NewInstance();
        conn.Sender = sender;
        conn.Receiver = receiver;
        flow.Connections.Add(conn);

        var copy = flow.AsReadOnly().AsModifiable();

        // the identity map is what makes this hold: the cross-reference must point INTO
        // the copy, not back at the original graph
        Assert.Same(copy.Nodes[0], copy.Connections[0].Sender);
        Assert.Same(copy.Nodes[1], copy.Connections[0].Receiver);
        Assert.NotSame(sender, copy.Connections[0].Sender);
    }

    [Fact]
    public void AsModifiable_IsCovariantOnSubtypes()
    {
        var dog = Dog.NewInstance();
        dog.Name = "Rex";
        dog.Breed = "Husky";

        // the derived read-only interface hands back the derived mutable type
        Dog copy = dog.AsReadOnly().AsModifiable();
        Assert.Equal("Husky", copy.Breed);

        // through the base read-only interface, the base type -- as Java's covariant
        // redeclaration gives
        ReadOnlyAnimal asAnimal = dog.AsReadOnly();
        Animal animalCopy = asAnimal.AsModifiable();
        Assert.Equal("Rex", animalCopy.Name);
        Assert.IsAssignableFrom<Dog>(animalCopy);
    }

    // ------------------------------------------------------------------
    // ContentHashCode, found untested by the audit (issue #2). ContentEquals was covered;
    // its hash partner was not, and the two have to agree or content-keyed dictionaries break.
    // ------------------------------------------------------------------

    [Fact]
    public void ContentHashCode_AgreesWithContentEquals()
    {
        var a = Flow.NewInstance();
        a.Title = "same";
        var b = Flow.NewInstance();
        b.Title = "same";

        Assert.True(a.VMF.Content.ContentEquals(b), "precondition: these are content-equal");
        Assert.Equal(a.VMF.Content.ContentHashCode(), b.VMF.Content.ContentHashCode());
    }

    [Fact]
    public void ContentHashCode_IsStableAcrossCalls()
    {
        var flow = Flow.NewInstance();
        flow.Title = "stable";
        flow.Nodes.Add(Node.NewInstance());

        Assert.Equal(flow.VMF.Content.ContentHashCode(), flow.VMF.Content.ContentHashCode());
    }

    [Fact]
    public void ContentHashCode_SurvivesADeepCopy()
    {
        var flow = Flow.NewInstance();
        flow.Title = "copied";
        var node = Node.NewInstance();
        node.Name = "n";
        flow.Nodes.Add(node);

        var copy = flow.VMF.Content.DeepCopy<Flow>();

        Assert.True(flow.VMF.Content.ContentEquals(copy));
        Assert.Equal(flow.VMF.Content.ContentHashCode(), copy.VMF.Content.ContentHashCode());
    }
}
