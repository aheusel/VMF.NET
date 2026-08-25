// Ported from eu.mihosoft.vmftest.recursivelistener01.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01.VmfModel;

interface INode
{
    [Container("INode.Children")]
    INode? Parent { get; }

    [Contains("INode.Parent")]
    INode[] Children { get; }

    string? Name { get; set; }
}
