// Ported from eu.mihosoft.vmftest.ignoretostring.ToStringTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.IgnoreToString;

public class ToStringTest
{
    [Fact]
    public void TestIgnoreTo()
    {
        var instance = ISampleClass.NewBuilder()
            .WithName("my name")
            .WithIgnoredProp("ignored prop")
            .Build();

        var toString = instance.ToString();

        Assert.Contains("my name", toString);
        Assert.DoesNotContain("ignored prop", toString);
    }
}
