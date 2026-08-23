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
    }
}
