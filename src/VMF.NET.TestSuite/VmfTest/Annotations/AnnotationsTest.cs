// Ported from eu.mihosoft.vmftest.annotations.AnnotationsTest

using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Annotations;

public class AnnotationsTest
{
    // NOT a deviation, though it was recorded as one until 2026-08-25.
    //
    // VMF.NET exposes "vmf:property:containment-info" on every property through the public
    // Annotations() list -- and so does Java, with identical values (none / contained:<opposite>
    // / container:<opposite>). Measured against vmf 0.2.9.7-SNAPSHOT; see VMF.NET.JavaProbe.
    //
    // Java's own AnnotationsTest is the tell: it asserts an EXACT size for type-level annotations
    // but filters by key for property-level ones. This filter exists for the same reason Java's
    // does, so the port stays faithful rather than diverging.
    //
    // ShallowCopyAnnotationTests asserts the bookkeeping annotation S visible, which is why the
    // filter lives here rather than in the runtime.
    private static System.Collections.Generic.List<VMF.NET.Runtime.IAnnotation> UserAnnotations(
        VMF.NET.Runtime.IVObject o, string propertyName) =>
        o.VMF.Reflect.PropertyByName(propertyName)!.Annotations()
            .Where(a => !a.Key.StartsWith("vmf:")).ToList();

    [Fact]
    public void BasicAnnotationTest()
    {
        var annotatedModel = AnnotatedModel.NewInstance();

        var annotations = annotatedModel.VMF.Reflect.Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my value 2"), $"Not as expected, got: {annotations[1]}");

        var propertyAnnotations = UserAnnotations(annotatedModel, "Name");

        Assert.Equal(2, propertyAnnotations.Count);
        Assert.True(propertyAnnotations[0].Equals("prop key 1", "my prop value 1"),
            $"Not as expected, got: {propertyAnnotations[0]}");
        Assert.True(propertyAnnotations[1].Equals("prop key 2", "my prop value 2"),
            $"Not as expected, got: {propertyAnnotations[1]}");

        Assert.NotNull(annotatedModel.VMF.Reflect.AnnotationByKey("key 1"));
        Assert.NotNull(annotatedModel.VMF.Reflect.AnnotationByKey("key 2"));
        Assert.Null(annotatedModel.VMF.Reflect.AnnotationByKey("key 3"));
    }

    [Fact]
    public void MultipleAnnotationsPerKeyTest()
    {
        var annotatedObject = MultipleAnnotationsPerKey.NewInstance();

        var annotations = annotatedObject.VMF.Reflect.Annotations();

        Assert.Equal(3, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.Equal("key 2", annotations[1].Key);
        Assert.Equal("key 2", annotations[2].Key);

        var key1 = annotatedObject.VMF.Reflect.AnnotationsByKey("key 1");
        var key2 = annotatedObject.VMF.Reflect.AnnotationsByKey("key 2");

        Assert.Single(key1);
        Assert.Equal(2, key2.Count);
    }

    [Fact]
    public void AnnotationInheritanceTest()
    {
        var annotatedObjectParent = AnnotationInheritance1Parent.NewInstance();

        var annotations = annotatedObjectParent.VMF.Reflect.Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my parent value 1"),
            $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my parent value 2"),
            $"Not as expected, got: {annotations[1]}");

        var annotatedObjectChild = AnnotationInheritance1Child.NewInstance();

        annotations = annotatedObjectChild.VMF.Reflect.Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my child value 1"),
            $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my child value 2"),
            $"Not as expected, got: {annotations[1]}");
    }

    [Fact]
    public void AnnotationPropertyInheritanceTest()
    {
        // annotations declared on a property are visible through reflection, and a subtype
        // that re-declares the property carries its own
        var parent = AnnotationInheritance2Parent.NewInstance();
        var parentAnnotations = UserAnnotations(parent, "Name");

        Assert.Equal(2, parentAnnotations.Count);
        Assert.Equal("key 1", parentAnnotations[0].Key);
        Assert.Equal("my parent value 1", parentAnnotations[0].Value);
        Assert.Equal("key 2", parentAnnotations[1].Key);
        Assert.Equal("my parent value 2", parentAnnotations[1].Value);

        var child = AnnotationInheritance2Child.NewInstance();
        var childAnnotations = UserAnnotations(child, "Name");

        Assert.Equal(2, childAnnotations.Count);
        Assert.Equal("key 1", childAnnotations[0].Key);
        Assert.Equal("my child value 1", childAnnotations[0].Value);
        Assert.Equal("key 2", childAnnotations[1].Key);
        Assert.Equal("my child value 2", childAnnotations[1].Value);
    }
}
