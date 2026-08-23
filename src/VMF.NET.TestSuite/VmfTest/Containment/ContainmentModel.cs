// Ported from eu.mihosoft.vmftest.containment.vmfmodel.ContainmentTest

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Containment;

[VmfModel]
public partial interface IContainerOne
{
    [Contains]
    IElement? Element { get; set; }

    [Contains("IElement.ParentOne")]
    IElement? Element1 { get; set; }

    [Contains]
    VList<IElement> Elements1 { get; }

    [Contains("IElement.ListParentOne")]
    VList<IElement> Elements1a { get; }
}

[VmfModel]
public partial interface IContainerTwo
{
    [Contains]
    IElement? Element { get; set; }

    [Contains("IElement.ParentTwo")]
    IElement? Element2 { get; set; }

    [Contains]
    VList<IElement> Elements2 { get; }

    [Contains("IElement.ListParentTwo")]
    VList<IElement> Elements2a { get; }
}

[VmfModel]
public partial interface IElement
{
    [Container("IContainerOne.Element1")]
    IContainerOne? ParentOne { get; }

    [Container("IContainerTwo.Element2")]
    IContainerTwo? ParentTwo { get; }

    [Container("IContainerOne.Elements1a")]
    IContainerOne? ListParentOne { get; }

    [Container("IContainerTwo.Elements2a")]
    IContainerTwo? ListParentTwo { get; }
}

[VmfModel]
public partial interface IContainerMultipleOpposites
{
    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites? Element { get; set; }

    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites? Element1 { get; set; }

    [Contains("IElementMultipleOpposites.Parent")]
    VList<IElementMultipleOpposites> Elements { get; }

    [Contains("IElementMultipleOpposites.Parent")]
    VList<IElementMultipleOpposites> Elements1 { get; }
}

[VmfModel]
public partial interface IElementMultipleOpposites
{
    // multiple opposites (unknown at compile-time)
    [Container]
    IContainerMultipleOpposites? Parent { get; }
}
