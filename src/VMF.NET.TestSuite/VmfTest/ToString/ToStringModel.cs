// Ported from eu.mihosoft.vmftest.tostring.vmfmodel.ToStringModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ToString.VmfModel;

// we want to test ToString() with circular dependency and without containment
interface IParent
{
    IChild? Child { get; set; }
    string? Name { get; set; }
}

interface IChild
{
    IParent? Parent { get; set; }
    string? Name { get; set; }
}

// and here we test with collections
interface IParent2
{
    IChild2[] Children { get; }
    string? Name { get; set; }
}

interface IChild2
{
    IParent2? Parent { get; set; }
    string? Name { get; set; }
}
