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
    private static List<IChange> RecordedInReverse(INode root)
    {
        var changes = new List<IChange>(root.Vmf().Changes().All());
        changes.Reverse();
        return changes;
    }

    [Fact]
    public void Undo_ScalarPropertyChange_RestoresTheOldValue()
    {
        var node = INode.NewInstance();
        node.Name = "before";

        node.Vmf().Changes().Start();
        node.Name = "after";

        Assert.Single(node.Vmf().Changes().All());

        node.Vmf().Changes().All()[0].Undo();

        Assert.Equal("before", node.Name);
    }

    [Fact]
    public void Undo_ListAdd_RemovesTheElement()
    {
        var root = INode.NewInstance();

        root.Vmf().Changes().Start();
        root.Children.Add(INode.NewInstance());

        Assert.Single(root.Children);

        root.Vmf().Changes().All()[0].Undo();

        Assert.Empty(root.Children);
    }

    [Fact]
    public void Undo_ListRemove_PutsTheElementBack()
    {
        var root = INode.NewInstance();
        var child = INode.NewInstance();
        child.Name = "c";
        root.Children.Add(child);

        root.Vmf().Changes().Start();
        root.Children.Remove(child);

        Assert.Empty(root.Children);

        root.Vmf().Changes().All()[0].Undo();

        Assert.Single(root.Children);
        Assert.Equal("c", root.Children[0].Name);
    }

    [Fact]
    public void Undo_AllChangesInReverse_ReturnsTheGraphToItsStartingState()
    {
        // the shape the VFlow fact relies on: record a build-up, then undo every change back
        var root = INode.NewInstance();
        root.Vmf().Changes().Start();

        for (int i = 0; i < 5; i++)
        {
            var child = INode.NewInstance();
            child.Name = "child " + i;
            root.Children.Add(child);

            for (int j = 0; j < 3; j++)
            {
                var grandChild = INode.NewInstance();
                grandChild.Name = $"gc {i}.{j}";
                child.Children.Add(grandChild);
            }
        }

        Assert.Equal(5, root.Children.Count);
        // 1 root + 5 children + 15 grandchildren, each visited once (UniqueNode)
        Assert.Equal(21, root.Vmf().Content().Stream<INode>().Count());

        foreach (var change in RecordedInReverse(root))
        {
            change.Undo();
        }

        Assert.Empty(root.Children);
        Assert.Single(root.Vmf().Content().Stream<INode>());
    }
}
