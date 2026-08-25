// Ported from eu.mihosoft.vmftest.observableprop.vmfmodel.ObservablePropTest

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ObservableProp.VmfModel;

interface ObserveMyProperties
{
    string? Name { get; set; }
    int[] Values { get; }
}
