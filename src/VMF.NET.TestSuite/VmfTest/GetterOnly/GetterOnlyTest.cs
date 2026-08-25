// Ported from eu.mihosoft.vmftest.getteronly.GetterOnlyTest
//
// A [GetterOnly] property is readable through the shared interface on both the immutable and
// the mutable type, but only the mutable one may be written reflectively.

using System;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.GetterOnly;

public class GetterOnlyTest
{
    [Fact]
    public void GetterOnly_ReadableOnBoth_WritableOnlyOnMutable()
    {
        var immutableObj = IImmutableObj.NewBuilder().WithName("immutable obj").Build();
        var mutableObj = IMutableObj.NewBuilder().WithName("mutable obj").Build();

        IWithName withName1 = immutableObj;
        IWithName withName2 = mutableObj;

        Assert.Equal("immutable obj", withName1.Name);
        Assert.Equal("mutable obj", withName2.Name);

        // setting the immutable property must fail
        var immutableProp = immutableObj.VMF.Reflect.PropertyByName("Name");
        Assert.NotNull(immutableProp);
        Assert.ThrowsAny<Exception>(() => immutableProp!.Set("new name"));

        // setting the mutable property must work
        var mutableProp = mutableObj.VMF.Reflect.PropertyByName("Name");
        Assert.NotNull(mutableProp);
        mutableProp!.Set("new name");
        Assert.Equal("new name", mutableObj.Name);
    }
}
