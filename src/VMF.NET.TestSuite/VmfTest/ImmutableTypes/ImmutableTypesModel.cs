// Ported from eu.mihosoft.vmftest.immutabletypes.vmfmodel.ImmutableTypes

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ImmutableTypes;

[VmfModel]
[Immutable]
public partial interface IImmutableType
{
    string? Name { get; }
}

// should compile, see https://github.com/miho/VMF/issues/48
[VmfModel]
[Immutable]
public partial interface IImmutableTypeWithList
{
    VList<string> Names { get; }
}
