// Ported from eu.mihosoft.vmftest.staticreflection.StaticReflectionTest
//
// DEVIATION: Java's static entry point is Root.type(). C# cannot use that name -- a model may
// declare a property called Type, and several do, and a method cannot share a name with a
// property on the same interface. The generated equivalent is GetModelType().

using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.StaticReflection;

public class StaticReflectionTest
{
    private const string Ns = "VMF.NET.TestSuite.VmfTest.StaticReflection.";

    [Fact]
    public void StaticReflectionTest_PropertiesAndSuperTypes()
    {
        var propSize = Root.GetModelType().Reflect().Properties().Count;

        Assert.Equal(1, propSize);

        var p = Root.GetModelType().Reflect().Properties()[0];

        // a property obtained without an instance cannot be written
        Assert.ThrowsAny<System.Exception>(() => p.Set(null));

        var superTypes = p.Type.SuperTypes();

        Assert.Equal(2, superTypes.Count);

        var typeNames = p.Type.SuperTypes().Select(t => t.Name).ToList();

        Assert.Equal(new[] { Ns + "TypeA", Ns + "TypeB" }, typeNames);

        propSize = TypeA.GetModelType().Reflect().Properties().Count;
        Assert.Equal(3, propSize);
        propSize = TypeB.GetModelType().Reflect().Properties().Count;
        Assert.Equal(3, propSize);
        propSize = TypeC.GetModelType().Reflect().Properties().Count;
        Assert.Equal(6, propSize);
    }
}
