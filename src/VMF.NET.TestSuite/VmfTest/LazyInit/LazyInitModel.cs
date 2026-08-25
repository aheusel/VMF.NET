// Ported from eu.mihosoft.vmftest.lazyinit.vmfmodel.LazyInit

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.LazyInit.VmfModel;

[VmfEquals]
interface IObj
{
    IEntry[] Entries { get; }
}

[VmfEquals]
interface IEntry
{
    string? Name { get; set; }
}
