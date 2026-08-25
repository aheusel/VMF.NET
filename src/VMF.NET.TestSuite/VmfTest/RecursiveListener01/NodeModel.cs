// Ported from eu.mihosoft.vmftest.recursivelistener01.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01.VmfModel;

interface Node
{
    [Container("Node.Children")]
    Node? Parent { get; }

    [Contains("Node.Parent")]
    Node[] Children { get; }

    string? Name { get; set; }
}
