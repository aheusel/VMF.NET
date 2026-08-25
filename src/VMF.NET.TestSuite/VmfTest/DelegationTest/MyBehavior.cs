// Ported from the MyBehavior source Java compiles at test time.
//
// ConstructorCalled() is what proves the type-level [DelegateTo] hook ran: the delegate is
// one instance per object, so the flag OnDelegationTestClassInstantiated sets is the one the
// method reads back.

using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.DelegationTest;

public sealed class MyBehavior : IDelegatedBehavior<IDelegationTestClass>
{
    private IDelegationTestClass? _caller;
    private bool _constructorCalled;

    public void SetCaller(IDelegationTestClass caller) => _caller = caller;

    public bool NameStartsWith(string value)
    {
        if (value == null)
        {
            return false;
        }

        return _caller!.Name!.StartsWith(value);
    }

    public void OnDelegationTestClassInstantiated()
    {
        _constructorCalled = true;
    }

    public bool ConstructorCalled() => _constructorCalled;
}
