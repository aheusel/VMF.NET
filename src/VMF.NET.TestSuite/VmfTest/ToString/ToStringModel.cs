// Ported from eu.mihosoft.vmftest.tostring.vmfmodel.ToStringModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ToString;

// we want to test ToString() with circular dependency and without containment
[VmfModel]
public partial interface IParent
{
    IChild? Child { get; set; }
    string? Name { get; set; }
}

[VmfModel]
public partial interface IChild
{
    IParent? Parent { get; set; }
    string? Name { get; set; }
}

// and here we test with collections
[VmfModel]
public partial interface IParent2
{
    VList<IChild2> Children { get; }
    string? Name { get; set; }
}

[VmfModel]
public partial interface IChild2
{
    IParent2? Parent { get; set; }
    string? Name { get; set; }
}
