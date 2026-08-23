// Ported from eu.mihosoft.vmftests.test2.vmfmodel (Named, Parent, Child)

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Test2;

[VmfModel]
[VmfEquals]
public partial interface INamed
{
    string? Name { get; set; }
}

[VmfModel]
[VmfEquals]
public partial interface IParent : INamed
{
    [Contains("IChild.Parent")]
    VList<IChild> Children { get; }

    VList<INamed> Elements { get; }
}

[VmfModel]
[VmfEquals]
public partial interface IChild : INamed
{
    [Container("IParent.Children")]
    IParent? Parent { get; }
}
