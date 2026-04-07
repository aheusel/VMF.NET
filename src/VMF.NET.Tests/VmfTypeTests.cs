using Xunit;
using VMF.NET.Runtime;

namespace VMF.NET.Tests;

public class VmfTypeTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var type = VmfType.Create(true, false, false, "MyApp.Models.Person");
        Assert.True(type.IsModelType);
        Assert.False(type.IsListType);
        Assert.False(type.IsInterfaceOnly);
        Assert.Equal("MyApp.Models.Person", type.Name);
    }

    [Fact]
    public void GetElementTypeName_ListType_ReturnsElement()
    {
        var type = VmfType.Create(true, true, false, "VList<MyApp.Models.Child>");
        Assert.Equal("MyApp.Models.Child", type.GetElementTypeName());
    }

    [Fact]
    public void GetElementTypeName_NonListType_ReturnsNull()
    {
        var type = VmfType.Create(true, false, false, "MyApp.Models.Person");
        Assert.Null(type.GetElementTypeName());
    }

    [Fact]
    public void Equality_SameNameAndFlags_AreEqual()
    {
        var a = VmfType.Create(true, false, false, "Foo");
        var b = VmfType.Create(true, false, false, "Foo");
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentName_AreNotEqual()
    {
        var a = VmfType.Create(true, false, false, "Foo");
        var b = VmfType.Create(true, false, false, "Bar");
        Assert.NotEqual(a, b);
    }
}
