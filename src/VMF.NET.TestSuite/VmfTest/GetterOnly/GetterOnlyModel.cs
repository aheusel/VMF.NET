// Ported from eu.mihosoft.vmftest.getteronly.vmfmodel.GetterOnlyModel

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.GetterOnly;

[VmfModel]
[InterfaceOnly]
public partial interface IWithName
{
    [GetterOnly]
    string? Name { get; }
}

[VmfModel]
[Immutable]
public partial interface IImmutableObj : IWithName
{
}

[VmfModel]
public partial interface IMutableObj : IWithName
{
}
