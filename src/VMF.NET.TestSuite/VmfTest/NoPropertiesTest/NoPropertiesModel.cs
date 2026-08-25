// Ported from eu.mihosoft.vmftests.nopropertiestest.vmfmodel.NoProperties

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.NoPropertiesTest.VmfModel;

interface NoProperties
{
    [DelegateTo(typeof(DelegatedBehavior))]
    void TestDelegation();
}
