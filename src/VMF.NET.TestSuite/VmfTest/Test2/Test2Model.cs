// Ported from eu.mihosoft.vmftests.test2.vmfmodel (Named, Parent, Child)

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Test2.VmfModel;

[VmfEquals]
interface Named
{
    string? Name { get; set; }
}

[VmfEquals]
interface Parent : Named
{
    [Contains("Child.Parent")]
    Child[] Children { get; }

    Named[] Elements { get; }
}

[VmfEquals]
interface Child : Named
{
    [Container("Parent.Children")]
    Parent? Parent { get; }
}
