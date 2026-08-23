// Ported from eu.mihosoft.vmftest.recursivelistener01.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01;

[VmfModel]
public partial interface INode
{
    [Container("INode.Children")]
    INode? Parent { get; }

    [Contains("INode.Parent")]
    VList<INode> Children { get; }

    string? Name { get; set; }
}
