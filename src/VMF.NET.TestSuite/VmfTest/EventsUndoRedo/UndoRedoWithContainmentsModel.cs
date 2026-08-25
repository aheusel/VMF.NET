// Ported from eu.mihosoft.vmftest.events_undo_redo.vmfmodel.UndoRedoWithContainments

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.EventsUndoRedo.VmfModel;

interface ParentListContainment
{
    [Contains("ChildListContainment.Parent")]
    ChildListContainment[] Children { get; }
}

interface ChildListContainment
{
    // settable: the Java facts drive containment from the child with setParent(parent)
    [Container("ParentListContainment.Children")]
    ParentListContainment? Parent { get; set; }

    string? Name { get; set; }
}

interface ParentSingleContainment
{
    [Contains("ChildSingleContainment.Parent")]
    ChildSingleContainment? Child { get; set; }
}

interface ChildSingleContainment
{
    // settable: the Java facts drive containment from the child with setParent(parent)
    [Container("ParentSingleContainment.Child")]
    ParentSingleContainment? Parent { get; set; }

    string? Name { get; set; }
}
