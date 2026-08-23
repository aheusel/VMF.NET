// Ported from eu.mihosoft.vmftest.lazyinit.vmfmodel.LazyInit

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.LazyInit;

[VmfModel]
[VmfEquals]
public partial interface IObj
{
    VList<IEntry> Entries { get; }
}

[VmfModel]
[VmfEquals]
public partial interface IEntry
{
    string? Name { get; set; }
}
