// Ported from eu.mihosoft.vmftest.recursivelistener01.RecursiveListenerTest
//
// A non-recursive listener sees only changes on the object it is registered on; a recursive
// one also sees changes anywhere in the contained subtree.

using System.Collections.Generic;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01;

public class RecursiveListenerTest
{
    [Fact]
    public void RecursiveVsNonRecursiveListenerTest()
    {
        // build a tree: root + 3 layers of 10 children each
        var root = INode.NewInstance();
        root.Name = "ROOT";

        var parents = new List<INode> { root };
        for (int depth = 0; depth < 3; depth++)
        {
            var layer = new List<INode>();
            foreach (var p in parents)
            {
                for (int i = 0; i < 10; i++)
                {
                    var n = INode.NewInstance();
                    n.Name = $"d={depth}, i={i}";
                    p.Children.Add(n);
                    layer.Add(n);
                }
            }
            parents = layer;
        }

        int nonRecursiveChanges = 0;
        root.VMF.Changes.AddListener(_ => nonRecursiveChanges++, recursive: false);

        int recursiveChanges = 0;
        root.VMF.Changes.AddListener(_ => recursiveChanges++, recursive: true);

        // changes on the root itself are seen by both
        root.Name = "root";
        Assert.Equal(1, nonRecursiveChanges);
        Assert.Equal(1, recursiveChanges);

        root.Children.Add(INode.NewBuilder().WithName("evt node").Build());
        Assert.Equal(2, nonRecursiveChanges);
        Assert.Equal(2, recursiveChanges);

        nonRecursiveChanges = 0;
        recursiveChanges = 0;

        // changes deep in the subtree are seen only by the recursive listener
        var descendant = root.Children[2].Children[7];
        descendant.Name = "my new name";
        Assert.Equal(0, nonRecursiveChanges);
        Assert.Equal(1, recursiveChanges);

        descendant.Children.Add(INode.NewBuilder().WithName("evt node").Build());
        Assert.Equal(0, nonRecursiveChanges);
        Assert.Equal(2, recursiveChanges);
    }

    [Fact]
    public void RegisterUnregisterSimpleProperties()
    {
        // a listener on the root sees a node's changes exactly while that node is reachable
        // from the root through CONTAINMENT -- a plain reference is not enough
        int changeCounter = 0;

        var root = NoContainment.INodeNoContainment.NewInstance();
        root.VMF.Changes.AddListener(change =>
        {
            if (change.PropertyName == "Name") changeCounter++;
        });

        var n1 = NoContainment.INodeNoContainment.NewInstance();

        root.Node = n1;

        // no event: Node is a plain reference property, so n1 is not contained
        n1.Name = "my name 0";
        Assert.Equal(0, changeCounter);
        changeCounter = 0;

        root.Children.Add(n1);

        // now contained, so the change reaches the root's listener
        n1.Name = "my name 1";
        Assert.Equal(1, changeCounter);
        changeCounter = 0;

        root.Node = null;

        // still contained via Children, so a path to the root remains
        n1.Name = "my name 2";
        n1.Name = "my name 3";
        Assert.Equal(2, changeCounter);
        changeCounter = 0;

        // detaching from the child side ends it
        n1.Parent = null;

        n1.Name = "my name 4";
        Assert.Equal(0, changeCounter);
    }
}
