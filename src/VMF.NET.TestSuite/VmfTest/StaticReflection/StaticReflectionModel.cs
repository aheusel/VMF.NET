// Ported from eu.mihosoft.vmftest.staticreflection.vmfmodel.StaticReflection

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.StaticReflection.VmfModel;

interface Root
{
    TypeC? Prop { get; set; }
}

interface TypeA : Root
{
    int PropA1 { get; set; }
    string? PropA2 { get; set; }
}

interface TypeB : Root
{
    double PropB1 { get; set; }
    TypeA? PropB2 { get; set; }
}

interface TypeC : TypeA, TypeB
{
    string? Name { get; set; }
}
