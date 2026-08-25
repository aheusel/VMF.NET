// Ported from eu.mihosoft.vmftest.tostring.vmfmodel.ToStringModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ToString.VmfModel;

// we want to test ToString() with circular dependency and without containment
interface Parent
{
    Child? Child { get; set; }
    string? Name { get; set; }
}

interface Child
{
    Parent? Parent { get; set; }
    string? Name { get; set; }
}

// and here we test with collections
interface Parent2
{
    Child2[] Children { get; }
    string? Name { get; set; }
}

interface Child2
{
    Parent2? Parent { get; set; }
    string? Name { get; set; }
}
