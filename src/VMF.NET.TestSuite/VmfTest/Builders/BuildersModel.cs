// Ported from eu.mihosoft.vmftest.builders.vmfmodel.Builders

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Builders.VmfModel;

interface AClass
{
    string? Name { get; set; }
    string[] Ids { get; }
    Child[] Children { get; }
    Child2? Child { get; set; }
}

interface Child
{
    int Value { get; set; }
}

interface Child2
{
    int Value { get; set; }
}
