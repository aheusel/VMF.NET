// Ported from eu.mihosoft.vmftest.recursivelistener01.nocontainment.vmfmodel.Node

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.RecursiveListener01.NoContainment;

[VmfModel]
public partial interface INodeNoContainment
{
    [Container("INodeNoContainment.Children")]
    INodeNoContainment? Parent { get; }

    [Contains("INodeNoContainment.Parent")]
    VList<INodeNoContainment> Children { get; }

    INodeNoContainment? Node { get; set; }

    string? Name { get; set; }
}
