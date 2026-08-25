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
        var gCode1 = GCode1.NewInstance();
        var p = gCode1.VMF.Reflect.PropertyByName("Location");
        Assert.NotNull(p);

        // ensure the property uses the correct type
        Assert.Equal(LocationXY.ModelType(), p!.Type);
    }
}
