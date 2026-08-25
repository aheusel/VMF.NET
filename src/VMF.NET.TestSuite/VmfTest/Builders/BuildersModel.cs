// Ported from eu.mihosoft.vmftest.builders.vmfmodel.Builders

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Builders.VmfModel;

interface IAClass
{
    string? Name { get; set; }
    string[] Ids { get; }
    IChild[] Children { get; }
    IChild2? Child { get; set; }
}

interface IChild
{
    int Value { get; set; }
}

interface IChild2
{
    int Value { get; set; }
}
