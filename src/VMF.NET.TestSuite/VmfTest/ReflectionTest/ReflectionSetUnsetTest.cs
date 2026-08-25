// Ported from eu.mihosoft.vmf.VMFGenerateRuns -- the facts that set up ReflectionTest/Node and
// the InheritedDefaultValue* models: five reflective set/unset facts and three inherited
// default value facts.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ReflectionTest;

public class ReflectionSetUnsetTest
{
    [Fact]
    public void TestReflectionSetUnsetPrimitiveWithCompiletimeDefault()
    {
        var aReflectionTest = IReflectionTest.NewInstance();

        // id must be equal to it's reflection property
        Assert.Equal(aReflectionTest.Id, aReflectionTest.VMF.Reflect.PropertyByName("Id")!.Get());

        // id must not be set
        Assert.False(aReflectionTest.VMF.Reflect.PropertyByName("Id")!.IsSet);

        // default value of id is 23
        Assert.Equal(23, aReflectionTest.Id);
    }

    [Fact]
    public void TestReflectionSetUnsetPrimitiveWithRuntimeDefault()
    {
        var aReflectionTest = IReflectionTest.NewInstance();

        // id2 is not set as well
        Assert.False(aReflectionTest.VMF.Reflect.PropertyByName("Id2")!.IsSet);

        // if we set id2 ...
        aReflectionTest.Id2 = "test 123";

        // ... it should be set
        Assert.True(aReflectionTest.VMF.Reflect.PropertyByName("Id2")!.IsSet);

        // if we set it to 'null' (the default value) ...
        aReflectionTest.Id2 = null;

        // ... it should not be set
        Assert.False(aReflectionTest.VMF.Reflect.PropertyByName("Id2")!.IsSet);

        // we should check per instance default values:
        aReflectionTest.VMF.Reflect.PropertyByName("Id2")!.SetDefault("abc");

        // the default value should be updated, so it should not be set
        Assert.False(aReflectionTest.VMF.Reflect.PropertyByName("Id2")!.IsSet);

        // ... but the value should be "abc" instead of "null"
        Assert.Equal("abc", aReflectionTest.Id2);
    }

    [Fact]
    public void TestReflectionSetUnsetCollectionWithCompiletimeDefault()
    {
        var aReflectionTest = IReflectionTest.NewInstance();

        // values is not set
        Assert.False(aReflectionTest.VMF.Reflect.PropertyByName("Values")!.IsSet);

        // (but is not null because of its default value, size==3)
        Assert.Equal(3, aReflectionTest.Values.Count);
    }

    [Fact]
    public void TestReflectionSetUnsetContainmentProperties()
    {
        var aNode = INode.NewInstance();

        // containment properties cannot be set. we expect unset as default:
        Assert.False(aNode.VMF.Reflect.PropertyByName("Parent")!.IsSet);

        // containment properties cannot be set. we expect an exception (for default values):
        Assert.ThrowsAny<System.Exception>(
            () => aNode.VMF.Reflect.PropertyByName("Parent")!.SetDefault(aNode));
    }

    [Fact]
    public void TestReflectionSetUnsetReadOnlyProperties()
    {
        var aReflectionTest = IReflectionTest.NewInstance();

        // obtain a readonly reference of the object:
        var aReflectionTestRO = aReflectionTest.AsReadOnly();

        // read-only properties cannot be set. we expect an exception:
        Assert.ThrowsAny<System.Exception>(
            () => aReflectionTestRO.VMF.Reflect.PropertyByName("Id")!.Set(24));

        // read-only properties cannot be set. we expect an exception (also for default values):
        Assert.ThrowsAny<System.Exception>(
            () => aReflectionTestRO.VMF.Reflect.PropertyByName("Id")!.SetDefault(25));
    }

    [Fact]
    public void TestInheritedDefaultValue()
    {
        // default should be set
        Assert.Equal(123, IInheritedDefaultValueParent.NewInstance().MyValue);

        // for inherited as well
        Assert.Equal(123, IInheritedDefaultValue.NewInstance().MyValue);
    }

    [Fact]
    public void TestInheritedDefaultValueWithOverride()
    {
        // for override we expect a different default value
        Assert.Equal(-123, IInheritedDefaultValueOverride.NewInstance().MyValue);

        // for override2 we expect a the default value of int since the feature was redeclared
        Assert.Equal(0, IInheritedDefaultValueOverride2.NewInstance().MyValue);
    }

    [Fact]
    public void TestInheritedDefaultValueMultipleParents()
    {
        // default should be set to inherited default of first interface
        // (order matters in extends I1, I2, ...)
        Assert.Equal(123, IInheritedDefaultValueFromTwoParents.NewInstance().MyValue);

        // default should be set to inherited default of first interface
        // (order matters in extends I1, I2, ...)
        Assert.Equal(456, IInheritedDefaultValueFromTwoParents2.NewInstance().MyValue);
    }
}
