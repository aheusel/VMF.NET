// Ported from eu.mihosoft.vmftest.staticreflection.vmfmodel.StaticReflection

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.StaticReflection.VmfModel;

interface IRoot
{
    ITypeC? Prop { get; set; }
}

interface ITypeA : IRoot
{
    int PropA1 { get; set; }
    string? PropA2 { get; set; }
}

interface ITypeB : IRoot
{
    double PropB1 { get; set; }
    ITypeA? PropB2 { get; set; }
}

interface ITypeC : ITypeA, ITypeB
{
    string? Name { get; set; }
}
