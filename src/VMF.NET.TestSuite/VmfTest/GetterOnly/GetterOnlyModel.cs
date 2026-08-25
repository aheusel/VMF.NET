// Ported from eu.mihosoft.vmftest.getteronly.vmfmodel.GetterOnlyModel

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.GetterOnly.VmfModel;

[InterfaceOnly]
interface IWithName
{
    [GetterOnly]
    string? Name { get; }
}

[Immutable]
interface IImmutableObj : IWithName
{
}

interface IMutableObj : IWithName
{
}
