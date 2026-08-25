// Ported from eu.mihosoft.vmftest.recursivelistener01.nocontainment.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01.NoContainment.VmfModel;

interface NodeNoContainment
{
    // Settable so the fact can detach a node with Parent = null (Java: setParent(null)).
    [Container("NodeNoContainment.Children")]
    NodeNoContainment? Parent { get; set; }

    [Contains("NodeNoContainment.Parent")]
    NodeNoContainment[] Children { get; }

    NodeNoContainment? Node { get; set; }

    string? Name { get; set; }
}
