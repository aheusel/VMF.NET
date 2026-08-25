// Ported from eu.mihosoft.vmf.VMFGenerateRuns.testMethodDelegation.
//
// Java compiles the delegate (MyBehavior) as a source string at test time via addCode(); the
// C# port declares it beside the model instead, which is where a VMF.NET delegate has to live.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.DelegationTest;

public class DelegationTest
{
    [Fact]
    public void TestMethodDelegation()
    {
        var aDelegationTestClass = DelegationTestClass.NewInstance();

        Assert.True(aDelegationTestClass.ConstructorCalled());

        aDelegationTestClass.Name = "Father";

        Assert.True(aDelegationTestClass.NameStartsWith("F"));
        Assert.False(aDelegationTestClass.NameStartsWith("G"));
    }
}
