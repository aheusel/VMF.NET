// Ported from eu.mihosoft.vmftest.events_undo_redo.UndoRedoWithContainmentTest
//
// Despite the package name these five facts never call undo(). They assert change RECORDING
// across a containment boundary: attaching a child produces exactly one recorded change on the
// parent, and once attached, the child's own changes are recorded on the parent too.
//
// Java's println calls inside the listeners are dropped; the counting they accompany is kept.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.EventsUndoRedo;

public class UndoRedoWithContainmentTest
{
    [Fact]
    public void UnoRedoWithListContainmentTestViaListAdd()
    {
        var parent = IParentListContainment.NewInstance();

        // count total change events
        int numChanges = 0;
        parent.VMF.Changes.AddListener(change => numChanges++);

        // start the change recording
        parent.VMF.Changes.Start();

        // create child
        var child = IChildListContainment.NewInstance();

        // count changes received by 'parent' properties
        int numChangesProp = 0;
        var parentProp = child.VMF.Reflect.PropertyByName("Parent");
        parentProp?.AddChangeListener(change => numChangesProp++);

        // add the child to the containment collection which sets the parent
        parent.Children.Add(child);

        // there's exactly one property 'parent' change
        // (the Java fact checks the TOTAL counter here, not the per-property one, although its
        // message says otherwise -- ported as written)
        Assert.Equal(1, numChanges);
        // there's exactly one undoable change
        Assert.Equal(1, parent.VMF.Changes.All().Count);
        // there is one change in total (second is fired only locally in child)
        Assert.Equal(1, numChanges);

        // set a child property and see if changes are recorded in parent
        child.Name = "my new name";

        // there are exactly two undoable change in the list
        Assert.Equal(2, parent.VMF.Changes.All().Count);
    }

    [Fact]
    public void UnoRedoWithListContainmentTestViaSetParent()
    {
        var parent = IParentListContainment.NewInstance();

        // count total change events
        int numChanges = 0;
        parent.VMF.Changes.AddListener(change => numChanges++);

        // start the change recording
        parent.VMF.Changes.Start();

        // create child
        var child = IChildListContainment.NewInstance();

        // count changes received by 'parent' properties
        int numChangesProp = 0;
        var parentProp = child.VMF.Reflect.PropertyByName("Parent");
        parentProp?.AddChangeListener(change => numChangesProp++);

        // set the parent which adds the child to the containment collection
        child.Parent = parent;

        // there's exactly one property 'parent' change
        Assert.Equal(1, numChangesProp);
        // there's exactly one undoable change
        Assert.Equal(1, parent.VMF.Changes.All().Count);
        // there is one changes in total (second is only fired locally in child)
        Assert.Equal(1, numChanges);

        // set a child property and see if changes are recorded in parent
        child.Name = "my new name";

        // there are exactly two undoable changes in the list
        Assert.Equal(2, parent.VMF.Changes.All().Count);
    }

    [Fact]
    public void UnoRedoWithSingleContainmentTest1()
    {
        var parent = IParentSingleContainment.NewInstance();

        // count total change events
        int numChanges = 0;
        parent.VMF.Changes.AddListener(change => numChanges++);

        // start the change recording
        parent.VMF.Changes.Start();

        // create child
        var child = IChildSingleContainment.NewInstance();

        // count changes received by 'parent' properties
        int numChangesProp = 0;
        var parentProp = child.VMF.Reflect.PropertyByName("Parent");
        parentProp?.AddChangeListener(change => numChangesProp++);

        // set child which sets the opposite as well
        parent.Child = child;

        // there's exactly one property 'parent' change
        Assert.Equal(1, numChangesProp);
        // there's exactly one undoable change
        Assert.Equal(1, parent.VMF.Changes.All().Count);
        // there is one change in total (second is only fired locally in child)
        Assert.Equal(1, numChanges);

        // set a child property and see if changes are recorded in parent
        child.Name = "my new name";

        // there are exactly two undoable changes in the list
        Assert.Equal(2, parent.VMF.Changes.All().Count);
    }

    [Fact]
    public void UnoRedoWithSingleContainmentTest2()
    {
        var parent = IParentSingleContainment.NewInstance();

        // count total change events
        int numChanges = 0;
        parent.VMF.Changes.AddListener(change => numChanges++);

        // start the change recording
        parent.VMF.Changes.Start();

        // create child
        var child = IChildSingleContainment.NewInstance();

        // count changes received by 'parent' properties
        int numChangesProp = 0;
        var parentProp = child.VMF.Reflect.PropertyByName("Parent");
        parentProp?.AddChangeListener(change => numChangesProp++);

        // set the parent which sets the opposite as well
        child.Parent = parent;

        // there's exactly one property 'parent' change
        Assert.Equal(1, numChangesProp);
        // there's exactly one undoable change
        Assert.Equal(1, parent.VMF.Changes.All().Count);
        // there is one change in total (second is fired only locally in child)
        Assert.Equal(1, numChanges);

        // set a child property and see if changes are recorded in parent
        child.Name = "my new name";

        // there are exactly two undoable changes in the list
        Assert.Equal(2, parent.VMF.Changes.All().Count);
    }

    [Fact]
    public void UnoRedoWithSingleContainmentTestwithadditionalListener()
    {
        var parent = IParentSingleContainment.NewInstance();

        // register non-recursive listener to reproduce issue #30
        // see https://github.com/miho/VMF/issues/30
        parent.VMF.Changes.AddListener(change => { }, recursive: false);

        // start the change recording
        parent.VMF.Changes.Start();

        // create child
        var child = IChildSingleContainment.NewInstance();

        // set the parent which sets the opposite as well
        child.Parent = parent;

        // there's exactly one undoable change
        Assert.Equal(1, parent.VMF.Changes.All().Count);

        // set a child property and see if changes are recorded in parent
        child.Name = "my new name";

        // there are exactly two undoable changes in the list
        Assert.Equal(2, parent.VMF.Changes.All().Count);
    }
}
