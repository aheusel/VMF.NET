// Ported from eu.mihosoft.vmftest.getteronly.vmfmodel.GetterOnlyModel

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.GetterOnly.VmfModel;

[InterfaceOnly]
interface WithName
{
    [GetterOnly]
    string? Name { get; }
}

[Immutable]
interface ImmutableObj : WithName
{
}

interface MutableObj : WithName
{
}
