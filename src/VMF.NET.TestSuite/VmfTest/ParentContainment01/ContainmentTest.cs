// Ported from eu.mihosoft.vmftest.parentcontainment01.ContainmentTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ParentContainment01;

public class ContainmentTest
{
    private const string Skip =
        "Needs type-level delegation to supply inherited members. The Java model declares no " +
        "[Contains]/[Container] at all: CodeEntity is [InterfaceOnly] with a type-level " +
        "@DelegateTo, and the delegate is what makes getParent()/root() work. VMF.NET generates " +
        "bodies only from method-level [DelegateTo] on the type itself, so Parent stays an " +
        "ordinary stored property and is never populated by setting Left/Right.";

    [Fact(Skip = Skip)]
    public void TestContainmentBehaviorGetParent()
    {
    }

    [Fact(Skip = Skip)]
    public void TestContainmentBehaviorFindRoot()
    {
    }
}
