// Ported from eu.mihosoft.vmftest.lazyinit.vmfmodel.LazyInit

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.LazyInit.VmfModel;

[VmfEquals]
interface Obj
{
    Entry[] Entries { get; }
}

[VmfEquals]
interface Entry
{
    string? Name { get; set; }
}
