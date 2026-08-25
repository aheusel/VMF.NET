// Ported from eu.mihosoft.vmftest.containment.vmfmodel.ContainmentTest

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Containment.VmfModel;

interface IContainerOne
{
    [Contains]
    IElement? Element { get; set; }

    [Contains("IElement.ParentOne")]
    IElement? Element1 { get; set; }

    [Contains]
    IElement[] Elements1 { get; }

    [Contains("IElement.ListParentOne")]
    IElement[] Elements1a { get; }
}

interface IContainerTwo
{
    [Contains]
    IElement? Element { get; set; }

    [Contains("IElement.ParentTwo")]
    IElement? Element2 { get; set; }

    [Contains]
    IElement[] Elements2 { get; }

    [Contains("IElement.ListParentTwo")]
    IElement[] Elements2a { get; }
}

interface IElement
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

interface IContainerMultipleOpposites
{
    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites? Element { get; set; }

    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites? Element1 { get; set; }

    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites[] Elements { get; }

    [Contains("IElementMultipleOpposites.Parent")]
    IElementMultipleOpposites[] Elements1 { get; }
}

interface IElementMultipleOpposites
{
    // multiple opposites (unknown at compile-time)
    [Container]
    IContainerMultipleOpposites? Parent { get; }
}
