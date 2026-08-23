// Ported from eu.mihosoft.vmftests.nopropertiestest.vmfmodel.NoProperties

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.NoPropertiesTest;

[VmfModel]
public partial interface INoProperties
{
    [DelegateTo(typeof(DelegatedBehavior))]
    void TestDelegation();
}

public sealed class DelegatedBehavior : IDelegatedBehavior<INoProperties>
{
    private INoProperties? _caller;
    public void SetCaller(INoProperties caller) => _caller = caller;

    public int CallCount { get; private set; }
    public void TestDelegation() => CallCount++;
}
