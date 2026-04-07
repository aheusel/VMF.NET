using Xunit;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;

namespace VMF.NET.Tests;

public class AnnotationImplTests
{
    [Fact]
    public void Equals_MatchingKeyAndValue_ReturnsTrue()
    {
        var a = new AnnotationImpl("key", "value");
        var b = new AnnotationImpl("key", "value");
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentKey_ReturnsFalse()
    {
        var a = new AnnotationImpl("key1", "value");
        var b = new AnnotationImpl("key2", "value");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void InterfaceEquals_MatchesKeyValue()
    {
        Runtime.IAnnotation a = new AnnotationImpl("api", "min=3");
        Assert.True(a.Equals("api", "min=3"));
        Assert.False(a.Equals("api", "max=5"));
    }

    [Fact]
    public void ToString_IncludesKeyAndValue()
    {
        var a = new AnnotationImpl("api", "min=3");
        Assert.Contains("api", a.ToString());
        Assert.Contains("min=3", a.ToString());
    }
}
