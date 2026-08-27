// Content traversal: a re-enumerable sequence for reading, a cursor for modifying.
//
// DescendantsAndSelf() replaced four Stream() overloads. Two of those merely wrapped LINQ's
// OfType<T>, and all of them were single-shot: the returned IEnumerable WAS the VIterator, whose
// GetEnumerator() returned itself, so a second enumeration silently yielded nothing. Measured
// before the fix: Count() gave 3, then 0.

using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.VmfTest.Containment;
using Xunit;

namespace VMF.NET.TestSuite;

public class ContentTraversalTests
{
    private static ContainerOne BuildGraph()
    {
        var root = ContainerOne.NewInstance();
        root.Elements1.Add(Element.NewInstance());
        root.Elements1.Add(Element.NewInstance());
        return root;
    }

    [Fact]
    public void DescendantsAndSelf_IncludesTheRootFirst()
    {
        var root = BuildGraph();

        var all = root.VMF.Content.DescendantsAndSelf().ToList();

        Assert.Equal(3, all.Count);
        Assert.Same(root, all[0]);
    }

    [Fact]
    public void DescendantsAndSelf_CanBeEnumeratedMoreThanOnce()
    {
        // The whole point of the rewrite. Storing the sequence and enumerating it twice is
        // ordinary LINQ usage, and it used to return the full graph and then nothing.
        var seq = BuildGraph().VMF.Content.DescendantsAndSelf();

        Assert.Equal(3, seq.Count());
        Assert.Equal(3, seq.Count());

        int first = 0, second = 0;
        foreach (var _ in seq) first++;
        foreach (var _ in seq) second++;

        Assert.Equal(3, first);
        Assert.Equal(3, second);
    }

    [Fact]
    public void OfType_SelectsByType_AndIsAlsoReEnumerable()
    {
        var typed = BuildGraph().VMF.Content.DescendantsAndSelf().OfType<Element>();

        Assert.Equal(2, typed.Count());
        Assert.Equal(2, typed.Count());
    }

    [Fact]
    public void ACursorIsConsumedOnce_AndIsNotASequence()
    {
        // A cursor is the modify-while-traversing tool and is deliberately NOT IEnumerable, so
        // the "enumerate twice, get nothing" trap cannot be written any more.
        var cursor = BuildGraph().VMF.Content.Cursor();

        int visited = 0;
        while (cursor.MoveNext()) visited++;

        Assert.Equal(3, visited);
        Assert.False(cursor is System.Collections.IEnumerable,
            "VIterator must not be IEnumerable: returning itself from GetEnumerator() is what made "
            + "a second enumeration silently empty.");
    }

    [Fact]
    public void ReadOnlyViews_TraverseTheSameWay()
    {
        var ro = BuildGraph().AsReadOnly();

        var seq = ((IVObject)ro).VMF.Content.DescendantsAndSelf();

        Assert.Equal(3, seq.Count());
        Assert.Equal(3, seq.Count());
    }
}
