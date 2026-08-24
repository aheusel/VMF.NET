// Ported from eu.mihosoft.vmftest.propertyinheritance.PropertyInheritanceTest
//
// Java's orElseThrow() becomes Assert.NotNull followed by `!`.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.PropertyInheritance;

public class PropertyInheritanceTest
{
    [Fact]
    public void PropertyInheritanceTest01()
    {
        var gCode1 = IGCode1.NewInstance();
        var p = gCode1.Vmf().Reflect().PropertyByName("Location");
        Assert.NotNull(p);

        // ensure the property uses the correct type
        Assert.Equal(ILocationXY.ModelType(), p!.Type);
    }
}
