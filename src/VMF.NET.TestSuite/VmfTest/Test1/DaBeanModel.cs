// Ported from eu.mihosoft.vmftests.test1.vmfmodel.DaBean

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Test1;

[VmfModel]
public partial interface IDaBean
{
    string? Name { get; set; }
}
