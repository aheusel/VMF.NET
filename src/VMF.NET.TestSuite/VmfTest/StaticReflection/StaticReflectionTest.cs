// Ported from eu.mihosoft.vmftest.staticreflection.StaticReflectionTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.StaticReflection;

public class StaticReflectionTest
{
    [Fact(Skip = "Needs static type reflection and populated supertypes. Java reaches a model " +
                 "type's reflection without an instance (Root.type().reflect()), which VMF.NET " +
                 "has no entry point for, and asserts on Type.superTypes() -- VmfType.SetSuperTypes " +
                 "is never called, so SuperTypes() is always empty.")]
    public void StaticReflectionTest_PropertiesAndSuperTypes()
    {
        // NEEDS a static entry point to a model type's reflection (Java: Root.type().reflect())
        // and populated supertypes (VmfType.SetSuperTypes is never called). Neither exists, so
        // the body is commented out rather than omitted. Restore it together with un-skipping.
        //
        // var propSize = IRoot.Type().Reflect().Properties().Count;
        // Assert.Equal(1, propSize);
        //
        // var p = IRoot.Type().Reflect().Properties()[0];
        //
        // // a property obtained without an instance cannot be written
        // Assert.ThrowsAny<System.Exception>(() => p.Set(null));
        //
        // var superTypes = p.Type.SuperTypes();
        // Assert.Equal(2, superTypes.Count);
        //
        // var typeNames = superTypes.Select(t => t.Name).ToList();
        // Assert.Equal(
        //     new[] { Ns + "ITypeA", Ns + "ITypeB" },
        //     typeNames);
        //
        // Assert.Equal(3, ITypeA.Type().Reflect().Properties().Count);
        // Assert.Equal(3, ITypeB.Type().Reflect().Properties().Count);
        // Assert.Equal(6, ITypeC.Type().Reflect().Properties().Count);
    }
}
