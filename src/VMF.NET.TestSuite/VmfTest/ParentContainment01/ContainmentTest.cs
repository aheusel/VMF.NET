// Ported from eu.mihosoft.vmftest.parentcontainment01.ContainmentTest
//
// Java's println calls are dropped. The @Rule Timeout(10s) has no xUnit equivalent worth
// reproducing: it guards against the root() walk looping forever, which the assertions below
// would surface anyway.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ParentContainment01;

public class ContainmentTest
{
    [Fact]
    public void TestContainmentBehaviorGetParent()
    {
        var operatorExpression = IOperatorExpression.NewInstance();

        var leftValue = INumberExpression.NewBuilder().WithValue(3.2).Build();
        var rightValue = INumberExpression.NewBuilder().WithValue(1.2).Build();

        Assert.Null(operatorExpression.Parent);
        Assert.Null(leftValue.Parent);
        Assert.Null(rightValue.Parent);

        operatorExpression.Left = leftValue;
        operatorExpression.Right = rightValue;

        Assert.Null(operatorExpression.Parent);
        Assert.Same(operatorExpression, leftValue.Parent);
        Assert.Same(operatorExpression, rightValue.Parent);
    }

    [Fact]
    public void TestContainmentBehaviorFindRoot()
    {
        var root = IOperatorExpression.NewInstance();

        var l0 = INumberExpression.NewBuilder().WithValue(3.2).Build();
        var r0 = IOperatorExpression.NewInstance();

        root.Left = l0;
        root.Right = r0;

        Assert.Same(root, l0.Root());
        Assert.Same(root, r0.Root());

        var l1 = INumberExpression.NewBuilder().WithValue(1.2).Build();
        var r1 = INumberExpression.NewBuilder().WithValue(5.6).Build();

        r0.Left = l1;
        r0.Right = r1;

        Assert.Same(root, l1.Root());
        Assert.Same(root, r1.Root());
    }
}
