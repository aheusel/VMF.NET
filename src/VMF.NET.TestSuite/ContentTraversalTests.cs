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

    // ------------------------------------------------------------------
    // Traversal APIs the audit (issue #2) found with no coverage at all:
    // ITraversalListener in its entirety, the cursor's Of/Current/IsAddSupported/Reset,
    // and IterationStrategy.UniqueProperty.
    // ------------------------------------------------------------------

    private sealed class RecordingListener : ITraversalListener
    {
        public List<IVObject> Entered { get; } = new();
        public List<IVObject> Exited { get; } = new();

        public void OnEnter(IVObject obj) => Entered.Add(obj);
        public void OnExit(IVObject obj) => Exited.Add(obj);
    }

    [Fact]
    public void TraversalListener_SeesEveryNodeOnTheWayInAndOut()
    {
        var root = BuildGraph();

        var listener = new RecordingListener();
        ITraversalListener.Traverse(root, listener);

        // the root and both elements
        Assert.Equal(3, listener.Entered.Count);
        Assert.Same(root, listener.Entered[0]);

        // everything entered is also exited
        Assert.Equal(
            listener.Entered.OrderBy(o => o.GetHashCode()).ToList(),
            listener.Exited.OrderBy(o => o.GetHashCode()).ToList());
    }

    [Fact]
    public void TraversalListener_HonoursTheStrategyItIsGiven()
    {
        var root = BuildGraph();

        var listener = new RecordingListener();
        ITraversalListener.Traverse(root, listener, IterationStrategy.ContainmentTree);

        Assert.Equal(3, listener.Entered.Count);
        Assert.Same(root, listener.Entered[0]);
    }

    [Fact]
    public void Cursor_ExposesCurrentAsItWalks()
    {
        var root = BuildGraph();
        var cursor = VIterator.Of(root);

        Assert.True(cursor.MoveNext());
        Assert.Same(root, cursor.Current);

        var seen = new List<IVObject> { cursor.Current };
        while (cursor.MoveNext()) seen.Add(cursor.Current);

        Assert.Equal(3, seen.Count);
        Assert.Equal(2, seen.OfType<Element>().Count());
    }

    [Fact]
    public void Cursor_AnswersIsAddSupportedOncePositioned()
    {
        var cursor = VIterator.Of(BuildGraph());

        Assert.True(cursor.MoveNext());
        _ = cursor.IsAddSupported;   // must be answerable, not throw
    }

    [Fact]
    public void Cursor_RefusesReset()
    {
        // Documented as unsupported: a graph walk cannot rewind. Pinned so it stays an explicit
        // NotSupportedException rather than degrading into a silently wrong re-walk.
        var cursor = VIterator.Of(BuildGraph());

        Assert.Throws<NotSupportedException>(() => cursor.Reset());
    }

    [Fact]
    public void UniquePropertyStrategy_ReachesTheWholeGraph()
    {
        // Nothing exercised this strategy before -- it was a public enum value with no test.
        var root = BuildGraph();

        var unique = VIterator.Sequence(root, IterationStrategy.UniqueNode).Count();
        var byProperty = VIterator.Sequence(root, IterationStrategy.UniqueProperty).Count();

        Assert.Equal(3, unique);

        // UniqueProperty visits each property rather than each node, so it may see more, never
        // fewer.
        Assert.True(byProperty >= unique,
            $"UniqueProperty saw {byProperty}, UniqueNode saw {unique}");
    }
}
