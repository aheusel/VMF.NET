// Ported from eu.mihosoft.vmftest.propertyinheritance.PropertyInheritanceTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.PropertyInheritance;

public class PropertyInheritanceTest
{
    [Fact(Skip = "Needs covariant property narrowing plus static type reflection. The fact " +
                 "asserts that GCode1.location reports type LocationXY, i.e. the narrowed type " +
                 "from the WithLocation chain; C# interfaces cannot override a property type, so " +
                 "the ported model keeps ILocation throughout. It also compares against " +
                 "LocationXY.type(), for which there is no static entry point.")]
    public void PropertyInheritanceTest01()
    {
    }
}
