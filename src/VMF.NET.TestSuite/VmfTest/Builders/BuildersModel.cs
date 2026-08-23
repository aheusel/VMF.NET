// Ported from eu.mihosoft.vmftest.builders.vmfmodel.Builders

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Builders;

[VmfModel]
public partial interface IAClass
{
    string? Name { get; set; }
    VList<string> Ids { get; }
    VList<IChild> Children { get; }
    IChild2? Child { get; set; }
}

[VmfModel]
public partial interface IChild
{
    int Value { get; set; }
}

[VmfModel]
public partial interface IChild2
{
    int Value { get; set; }
}
