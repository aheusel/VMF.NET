// Ported from eu.mihosoft.vmftest.recursivelistener01.nocontainment.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01.NoContainment.VmfModel;

interface INodeNoContainment
{
    // Settable so the fact can detach a node with Parent = null (Java: setParent(null)).
    [Container("INodeNoContainment.Children")]
    INodeNoContainment? Parent { get; set; }

    [Contains("INodeNoContainment.Parent")]
    INodeNoContainment[] Children { get; }

    INodeNoContainment? Node { get; set; }

    string? Name { get; set; }
}
