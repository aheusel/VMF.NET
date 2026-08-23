// Ported from eu.mihosoft.vmftests.delegationtest.vmfmodel.DelegationTestClass

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.DelegationTest;

[VmfModel]
[DelegateTo(typeof(MyBehavior))]
public partial interface IDelegationTestClass
{
    string? Name { get; set; }

    [DelegateTo(typeof(MyBehavior))]
    bool NameStartsWith(string value);

    [DelegateTo(typeof(MyBehavior))]
    bool ConstructorCalled();
}

public sealed class MyBehavior : IDelegatedBehavior<IDelegationTestClass>
{
    private IDelegationTestClass? _caller;
    private bool _constructorCalled;

    public MyBehavior() => _constructorCalled = true;

    public void SetCaller(IDelegationTestClass caller) => _caller = caller;

    public bool NameStartsWith(string value) => _caller?.Name?.StartsWith(value) ?? false;

    public bool ConstructorCalled() => _constructorCalled;
}
