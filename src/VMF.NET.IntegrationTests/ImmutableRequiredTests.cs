using VMF.NET.IntegrationTests.Models;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;
using Xunit;

namespace VMF.NET.IntegrationTests;

/// <summary>
/// Tests for immutable types and required property validation.
/// </summary>
public class ImmutableRequiredTests
{
    // --- Immutable type tests ---

    [Fact]
    public void Immutable_NewInstance_CreatesDefaultInstance()
    {
        var point = IPoint.NewInstance();
        Assert.Equal(0.0, point.X);
        Assert.Equal(0.0, point.Y);
    }

    [Fact]
    public void Immutable_Builder_SetsProperties()
    {
        var point = IPoint.NewBuilder()
            .WithX(3.0)
            .WithY(4.0)
            .Build();

        Assert.Equal(3.0, point.X);
        Assert.Equal(4.0, point.Y);
    }

    [Fact]
    public void Immutable_Clone_ReturnsSelf()
    {
        var point = IPoint.NewBuilder().WithX(1.0).WithY(2.0).Build();
        var clone = point.Clone();

        Assert.Same(point, clone);
    }

    [Fact]
    public void Immutable_AsReadOnly_ReturnsSelf()
    {
        var point = IPoint.NewBuilder().WithX(1.0).WithY(2.0).Build();
        var ro = point.AsReadOnly();

        Assert.Same(point, ro);
    }

    [Fact]
    public void Immutable_IsReadOnly_ReturnsTrue()
    {
        var point = IPoint.NewInstance();
        var intern = (IVObjectInternal)point;

        Assert.True(intern.IsReadOnly);
    }

    [Fact]
    public void Immutable_ContentEquals_Works()
    {
        var p1 = IPoint.NewBuilder().WithX(3.0).WithY(4.0).Build();
        var p2 = IPoint.NewBuilder().WithX(3.0).WithY(4.0).Build();

        Assert.Equal(p1, p2);
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void Immutable_ContentNotEquals_Works()
    {
        var p1 = IPoint.NewBuilder().WithX(1.0).WithY(2.0).Build();
        var p2 = IPoint.NewBuilder().WithX(3.0).WithY(4.0).Build();

        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void Immutable_ToString_ContainsValues()
    {
        var point = IPoint.NewBuilder().WithX(3.14).WithY(2.71).Build();
        var str = point.ToString();

        Assert.Contains("3.14", str);
        Assert.Contains("2.71", str);
    }

    [Fact]
    public void Immutable_Annotations_IncludesImmutableMarker()
    {
        var point = IPoint.NewInstance();
        var intern = (IVObjectInternal)point;
        var annotations = intern.GetAnnotations();

        Assert.Contains(annotations, a => a.Key == "vmf:type:immutable");
    }

    [Fact]
    public void Immutable_Changes_Throws()
    {
        var point = IPoint.NewInstance();
        Assert.Throws<InvalidOperationException>(() => point.Vmf().Changes());
    }

    [Fact]
    public void Immutable_SetPropertyValueById_Throws()
    {
        var point = IPoint.NewInstance();
        var mod = (IVObjectInternalModifiable)point;

        Assert.Throws<InvalidOperationException>(() => mod.SetPropertyValueById(0, 5.0));
    }

    [Fact]
    public void Immutable_Reflect_Works()
    {
        var point = IPoint.NewBuilder().WithX(1.0).WithY(2.0).Build();
        var reflect = point.Vmf().Reflect();

        Assert.Equal(2, reflect.Properties().Count);
        Assert.Contains(reflect.Properties(), p => p.Name == "X");
        Assert.Contains(reflect.Properties(), p => p.Name == "Y");
    }

    [Fact]
    public void Immutable_DeepCopy_ReturnsSelf()
    {
        var point = IPoint.NewBuilder().WithX(1.0).WithY(2.0).Build();
        var copy = point.Vmf().Content().DeepCopy<IPoint>();

        Assert.Same(point, copy);
    }

    [Fact]
    public void Immutable_ImplementsIImmutable()
    {
        var point = IPoint.NewInstance();
        Assert.IsAssignableFrom<IImmutable>(point);
    }

    // --- VmfRequired tests ---

    [Fact]
    public void Required_Builder_ThrowsWhenNotSet()
    {
        var builder = IShape.NewBuilder();
        // Name is required but not set
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Required_Builder_SucceedsWhenSet()
    {
        var shape = IShape.NewBuilder()
            .WithName("Triangle")
            .Build();

        Assert.Equal("Triangle", shape.Name);
    }

    [Fact]
    public void Required_Builder_WithPoints()
    {
        var p1 = IPoint.NewBuilder().WithX(0).WithY(0).Build();
        var p2 = IPoint.NewBuilder().WithX(1).WithY(0).Build();

        var shape = IShape.NewBuilder()
            .WithName("Line")
            .WithPoints(p1, p2)
            .Build();

        Assert.Equal("Line", shape.Name);
        Assert.Equal(2, shape.Points.Count);
    }

    [Fact]
    public void Shape_IsMutable()
    {
        var shape = IShape.NewBuilder().WithName("Test").Build();
        Assert.IsAssignableFrom<IMutable>(shape);

        // Can change name
        shape.Name = "Changed";
        Assert.Equal("Changed", shape.Name);
    }
}
