// Ported from eu.mihosoft.vmftest.immutabletypes.ImmutableTypesTest
//
// Java scans the generated interface for methods starting with "set". The C# equivalent is a
// property with no setter, so the port asserts on the property accessors instead.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ImmutableTypes;

public class ImmutableTypesTest
{
    [Fact]
    public void ImmutableType_InterfaceExposesNoSetters()
    {
        var type = typeof(IImmutableType);

        var name = type.GetProperty("Name");
        Assert.NotNull(name);
        Assert.True(name!.CanRead, "Name must be readable");
        Assert.False(name.CanWrite, "an immutable type must not expose a setter for Name");

        foreach (var p in type.GetProperties())
        {
            Assert.False(p.CanWrite, $"immutable interface must expose no setter, but {p.Name} has one");
        }
    }
}
