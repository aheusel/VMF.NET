// Content traversal: a re-enumerable sequence for reading, a cursor for modifying.
//
// Traverse() replaced four Stream() overloads. Two of those merely wrapped LINQ's
// OfType<T>, and all of them were single-shot: the returned IEnumerable WAS the VIterator, whose
// GetEnumerator() returned itself, so a second enumeration silently yielded nothing. Measured
// before the fix: Count() gave 3, then 0.

using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.VmfTest.Containment;
using VMF.NET.TestSuite.VmfTest.CrossRef;
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
    public void Traverse_IncludesTheRootFirst()
    {
        var root = BuildGraph();

        var all = root.VMF.Content.Traverse().ToList();

        Assert.Equal(3, all.Count);
        Assert.Same(root, all[0]);
    }

    [Fact]
    public void Traverse_CanBeEnumeratedMoreThanOnce()
    {
        // The whole point of the rewrite. Storing the sequence and enumerating it twice is
        // ordinary LINQ usage, and it used to return the full graph and then nothing.
        var seq = BuildGraph().VMF.Content.Traverse();

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
        var typed = BuildGraph().VMF.Content.Traverse().OfType<Element>();

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

        // Asserted against the TYPE, not the instance. `cursor is IEnumerable` would be a
        // compile-time-constant false today (CS0184) -- a dead assertion that also warns.
        // This form stays a real check, and still fails if IEnumerable is ever put back.
        Assert.DoesNotContain(typeof(System.Collections.IEnumerable),
            typeof(VIterator).GetInterfaces());
    }

    [Fact]
    public void Traverse_FollowsReferences_NotJustContainment()
    {
        // The default strategy walks every model-typed property, so it reaches objects that are
        // NOT descendants. This is why the method is called Traverse and not DescendantsAndSelf:
        // `a` and `b` are joined by a [Refers] cross-reference with no containment between them.
        var a = EntityOneA.NewInstance();
        var b = EntityTwoA.NewInstance();
        a.Ref = b;

        var graph = a.VMF.Content.Traverse().ToList();
        var tree = a.VMF.Content.Traverse(IterationStrategy.ContainmentTree).ToList();

        Assert.Equal(2, graph.Count);
        Assert.Contains(b, graph);

        // ContainmentTree is the one that really means "descendants".
        Assert.Single(tree);
        Assert.Same(a, tree[0]);
    }

    [Fact]
    public void ReadOnlyViews_TraverseTheSameWay()
    {
        var ro = BuildGraph().AsReadOnly();

        var seq = ((IVObject)ro).VMF.Content.Traverse();

        Assert.Equal(3, seq.Count());
        Assert.Equal(3, seq.Count());
    }
}
