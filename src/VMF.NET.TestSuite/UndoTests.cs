// Undo had an implemented API and no tests at all. These pin what it does on the two change
// kinds before anything relies on it.

using System.Collections.Generic;
using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.VmfTest.RecursiveListener01;
using Xunit;

namespace VMF.NET.TestSuite;

public class UndoTests
{
    private static List<IChange> RecordedInReverse(Node root)
    {
        var changes = new List<IChange>(root.VMF.Changes.All());
        changes.Reverse();
        return changes;
    }

    [Fact]
    public void Undo_ScalarPropertyChange_RestoresTheOldValue()
    {
        var node = Node.NewInstance();
        node.Name = "before";

        node.VMF.Changes.Start();
        node.Name = "after";

        Assert.Single(node.VMF.Changes.All());

        node.VMF.Changes.All()[0].Undo();

        Assert.Equal("before", node.Name);
    }

    [Fact]
    public void Undo_ListAdd_RemovesTheElement()
    {
        var root = Node.NewInstance();

        root.VMF.Changes.Start();
        root.Children.Add(Node.NewInstance());

        Assert.Single(root.Children);

        root.VMF.Changes.All()[0].Undo();

        Assert.Empty(root.Children);
    }

    [Fact]
    public void Undo_ListRemove_PutsTheElementBack()
    {
        var root = Node.NewInstance();
        var child = Node.NewInstance();
        child.Name = "c";
        root.Children.Add(child);

        root.VMF.Changes.Start();
        root.Children.Remove(child);

        Assert.Empty(root.Children);

        root.VMF.Changes.All()[0].Undo();

        Assert.Single(root.Children);
        Assert.Equal("c", root.Children[0].Name);
    }

    [Fact]
    public void Undo_AllChangesInReverse_ReturnsTheGraphToItsStartingState()
    {
        // the shape the VFlow fact relies on: record a build-up, then undo every change back
        var root = Node.NewInstance();
        root.VMF.Changes.Start();

        for (int i = 0; i < 5; i++)
        {
            var child = Node.NewInstance();
            child.Name = "child " + i;
            root.Children.Add(child);

            for (int j = 0; j < 3; j++)
            {
                var grandChild = Node.NewInstance();
                grandChild.Name = $"gc {i}.{j}";
                child.Children.Add(grandChild);
            }
        }

        Assert.Equal(5, root.Children.Count);
        // 1 root + 5 children + 15 grandchildren, each visited once (UniqueNode)
        Assert.Equal(21, root.VMF.Content.Traverse().OfType<Node>().Count());

        foreach (var change in RecordedInReverse(root))
        {
            change.Undo();
        }

        Assert.Empty(root.Children);
        Assert.Single(root.VMF.Content.Traverse().OfType<Node>());
    }

    // ------------------------------------------------------------------
    // Apply and IsUndoable, which the audit (issue #2) found untested. Apply replays a recorded
    // change onto a DIFFERENT object -- the building block Java's ModelDiff.apply is built from,
    // and the piece VMF.NET would need if ModelDiff is ever ported.
    // ------------------------------------------------------------------

    [Fact]
    public void Apply_ReplaysAScalarChangeOntoAnotherObject()
    {
        var source = Node.NewInstance();
        source.VMF.Changes.Start();
        source.Name = "recorded";

        var target = Node.NewInstance();
        target.Name = "untouched";

        var change = source.VMF.Changes.All().Single(c => c.PropertyChange is not null);
        change.Apply(target);

        Assert.Equal("recorded", target.Name);
        Assert.Equal("recorded", source.Name);   // the source is not disturbed
    }

    [Fact]
    public void Apply_ReplaysAListChangeOntoAnotherObject()
    {
        var source = Node.NewInstance();
        source.VMF.Changes.Start();
        var child = Node.NewInstance();
        child.Name = "c";
        source.Children.Add(child);

        var target = Node.NewInstance();

        var change = source.VMF.Changes.All().Single(c => c.ListChange is not null);
        change.Apply(target);

        Assert.Single(target.Children);
    }

    [Fact]
    public void RecordedChanges_ReportWhetherTheyCanBeUndone()
    {
        var node = Node.NewInstance();
        node.VMF.Changes.Start();
        node.Name = "x";
        node.Children.Add(Node.NewInstance());

        var changes = node.VMF.Changes.All();
        Assert.NotEmpty(changes);

        // Undo() is exercised throughout this file, so every recorded change must say so.
        Assert.All(changes, c => Assert.True(c.IsUndoable,
            "a recorded change that Undo() handles must report IsUndoable"));
    }
}
