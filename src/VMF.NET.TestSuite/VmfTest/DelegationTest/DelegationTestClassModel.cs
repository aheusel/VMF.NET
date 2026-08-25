// Ported from eu.mihosoft.vmftests.delegationtest.vmfmodel.DelegationTestClass and the
// MyBehavior source Java compiles at test time.
//
// ConstructorCalled() is what proves the type-level [DelegateTo] hook ran: the delegate is one
// instance per object, so the flag OnDelegationTestClassInstantiated sets is the one the method
// reads back.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.DelegationTest.VmfModel;

[DelegateTo(typeof(MyBehavior))]
interface DelegationTestClass
{
    string? Name { get; set; }

    [DelegateTo(typeof(MyBehavior))]
    bool NameStartsWith(string value);

    [DelegateTo(typeof(MyBehavior))]
    bool ConstructorCalled();
}
