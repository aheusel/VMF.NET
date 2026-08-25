// Ported from eu.mihosoft.vmftests.test2.vmfmodel (Named, Parent, Child)

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Test2.VmfModel;

[VmfEquals]
interface INamed
{
    string? Name { get; set; }
}

[VmfEquals]
interface IParent : INamed
{
    [Contains("IChild.Parent")]
    IChild[] Children { get; }

    INamed[] Elements { get; }
}

[VmfEquals]
interface IChild : INamed
{
    [Container("IParent.Children")]
    IParent? Parent { get; }
}
