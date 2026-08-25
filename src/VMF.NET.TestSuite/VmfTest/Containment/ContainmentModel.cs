// Ported from eu.mihosoft.vmftest.containment.vmfmodel.ContainmentTest

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Containment.VmfModel;

interface ContainerOne
{
    [Contains]
    Element? Element { get; set; }

    [Contains("Element.ParentOne")]
    Element? Element1 { get; set; }

    [Contains]
    Element[] Elements1 { get; }

    [Contains("Element.ListParentOne")]
    Element[] Elements1a { get; }
}

interface ContainerTwo
{
    [Contains]
    Element? Element { get; set; }

    [Contains("Element.ParentTwo")]
    Element? Element2 { get; set; }

    [Contains]
    Element[] Elements2 { get; }

    [Contains("Element.ListParentTwo")]
    Element[] Elements2a { get; }
}

interface Element
{
    [Container("ContainerOne.Element1")]
    ContainerOne? ParentOne { get; }

    [Container("ContainerTwo.Element2")]
    ContainerTwo? ParentTwo { get; }

    [Container("ContainerOne.Elements1a")]
    ContainerOne? ListParentOne { get; }

    [Container("ContainerTwo.Elements2a")]
    ContainerTwo? ListParentTwo { get; }
}

interface ContainerMultipleOpposites
{
    [Contains("ElementMultipleOpposites.Parent")]
    ElementMultipleOpposites? Element { get; set; }

    [Contains("ElementMultipleOpposites.Parent")]
    ElementMultipleOpposites? Element1 { get; set; }

    [Contains("ElementMultipleOpposites.Parent")]
    ElementMultipleOpposites[] Elements { get; }

    [Contains("ElementMultipleOpposites.Parent")]
    ElementMultipleOpposites[] Elements1 { get; }
}

interface ElementMultipleOpposites
{
    // multiple opposites (unknown at compile-time)
    [Container]
    ContainerMultipleOpposites? Parent { get; }
}
