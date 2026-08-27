// Ported from eu.mihosoft.vmftest.annotations.AnnotationsTest

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Annotations;

public class AnnotationsTest
{
    // Java reads a property's annotations RAW. Both sides emit VMF's own
    // "vmf:property:containment-info" on every property, and Java copes by counting only the keys
    // it cares about and then indexing positionally.
    //
    // This port did the same job by filtering out every "vmf:" key before counting and indexing.
    // That worked, but it was a rewrite rather than a translation, and it quietly dropped
    // something Java pins: that the user's annotations come FIRST, ahead of the bookkeeping entry.
    // Indexing into a filtered list cannot tell the difference. Verified 2026-08-25 in the
    // generated _VMF_PROPERTY_ANNOTATIONS: user annotations, then containment-info.
    private static IReadOnlyList<VMF.NET.Runtime.IAnnotation> PropertyAnnotations(
        VMF.NET.Runtime.IVObject o, string propertyName) =>
        o.VMF.Reflect.PropertyByName(propertyName)!.Annotations();

    [Fact]
    public void BasicAnnotationTest()
    {
        var annotatedModel = AnnotatedModel.NewInstance();

        var annotations = annotatedModel.VMF.Reflect.Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my value 2"), $"Not as expected, got: {annotations[1]}");

        var propertyAnnotations = PropertyAnnotations(annotatedModel, "Name");

        Assert.Equal(2, propertyAnnotations.Count(a => a.Key == "prop key 1" || a.Key == "prop key 2"));
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
        Assert.True(annotations[0].Equals("key 1", "my parent value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my parent value 2"), $"Not as expected, got: {annotations[1]}");

        var annotatedObjectChild = AnnotationInheritance1Child.NewInstance();

        annotations = annotatedObjectChild.VMF.Reflect.Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my child value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my child value 2"), $"Not as expected, got: {annotations[1]}");
    }

    [Fact]
    public void AnnotationPropertyInheritanceTest()
    {
        // annotations declared on a property are visible through reflection, and a subtype
        // that re-declares the property carries its own
        var parent = AnnotationInheritance2Parent.NewInstance();
        var annotations = PropertyAnnotations(parent, "Name");

        Assert.Equal(2, annotations.Count(a => a.Key == "key 1" || a.Key == "key 2"));
        Assert.True(annotations[0].Equals("key 1", "my parent value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my parent value 2"), $"Not as expected, got: {annotations[1]}");

        var annotatedObjectChild = AnnotationInheritance2Child.NewInstance();
        annotations = PropertyAnnotations(annotatedObjectChild, "Name");

        Assert.Equal(2, annotations.Count(a => a.Key == "key 1" || a.Key == "key 2"));
        Assert.True(annotations[0].Equals("key 1", "my child value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my child value 2"), $"Not as expected, got: {annotations[1]}");
    }
}
