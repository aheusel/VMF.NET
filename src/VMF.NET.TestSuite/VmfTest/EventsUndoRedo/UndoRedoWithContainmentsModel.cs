// Ported from eu.mihosoft.vmftest.events_undo_redo.vmfmodel.UndoRedoWithContainments

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.EventsUndoRedo;

[VmfModel]
public partial interface IParentListContainment
{
    [Contains("IChildListContainment.Parent")]
    VList<IChildListContainment> Children { get; }
}

[VmfModel]
public partial interface IChildListContainment
{
    // settable: the Java facts drive containment from the child with setParent(parent)
    [Container("IParentListContainment.Children")]
    IParentListContainment? Parent { get; set; }

    string? Name { get; set; }
}

[VmfModel]
public partial interface IParentSingleContainment
{
    [Contains("IChildSingleContainment.Parent")]
    IChildSingleContainment? Child { get; set; }
}

[VmfModel]
public partial interface IChildSingleContainment
{
    // settable: the Java facts drive containment from the child with setParent(parent)
    [Container("IParentSingleContainment.Child")]
    IParentSingleContainment? Parent { get; set; }

    string? Name { get; set; }
}
