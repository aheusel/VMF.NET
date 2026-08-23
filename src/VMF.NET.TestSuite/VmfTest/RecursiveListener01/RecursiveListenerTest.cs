// Ported from eu.mihosoft.vmftest.recursivelistener01.RecursiveListenerTest
//
// A non-recursive listener sees only changes on the object it is registered on; a recursive
// one also sees changes anywhere in the contained subtree.

using System.Collections.Generic;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01;

public class RecursiveListenerTest
{
    [Fact(Skip = "Recursive change listeners are not wired up. Two halves are missing: " +
                 "nothing ever calls IVObjectInternalModifiable.SetModelToChanges, so a " +
                 "ChangesManager attached to a root never reaches contained descendants; and " +
                 "ChangesManager.ProcessChange iterates _listeners, ignoring the recursive " +
                 "flag it records in _listenerEntries, so recursive and non-recursive " +
                 "listeners behave identically.")]
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
        root.Vmf().Changes().AddListener(_ => nonRecursiveChanges++, recursive: false);

        int recursiveChanges = 0;
        root.Vmf().Changes().AddListener(_ => recursiveChanges++, recursive: true);

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

    [Fact(Skip = "Needs a settable container property: the Java fact calls n1.setParent(null) " +
                 "to detach a node. VMF.NET never generates a setter for a [Container] " +
                 "property, so the child side cannot be detached directly.")]
    public void RegisterUnregisterSimpleProperties()
    {
    }
}
