// Ported from eu.mihosoft.vmftest.observableprop.vmfmodel.ObservablePropTest

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ObservableProp.VmfModel;

interface IObserveMyProperties
{
    string? Name { get; set; }
    VList<int> Values { get; }
}
