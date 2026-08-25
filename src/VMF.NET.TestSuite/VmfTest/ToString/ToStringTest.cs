// Ported from eu.mihosoft.vmftest.tostring.ToStringTest
//
// The Java facts only print the result; what they really guard is that ToString() terminates
// on a circular object graph instead of recursing forever. The ports assert that explicitly.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ToString;

public class ToStringTest
{
    [Fact]
    public void TestToStringCircular()
    {
        var p = Parent.NewInstance();
        p.Name = "parent";
        var c = Child.NewInstance();
        c.Name = "child";
        p.Child = c;
        c.Parent = p;

        var str = p.ToString();

        Assert.False(string.IsNullOrWhiteSpace(str));
        Assert.Contains("parent", str);
        Assert.Contains("child", str);
    }

    [Fact]
    public void TestToStringCircularCollection()
    {
        var p = Parent2.NewInstance();
        p.Name = "parent";
        var c = Child2.NewInstance();
        c.Name = "child";
        p.Children.Add(c);
        c.Parent = p;

        var str = p.ToString();

        Assert.False(string.IsNullOrWhiteSpace(str));
        Assert.Contains("parent", str);
        Assert.Contains("child", str);
    }
}
