// Ported from eu.mihosoft.vmftest.annotations.AnnotationsTest

using System.Linq;
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Annotations;

public class AnnotationsTest
{
    // DEVIATION: VMF.NET also exposes its own bookkeeping annotations -- per-property
    // "vmf:property:containment-info", and per-type "vmf:type:immutable"/"vmf:type:interface-only"
    // for those kinds of type -- through the public Annotations() lists. None of the types in
    // this area is immutable or interface-only, so the type-level counts below match Java; the
    // property-level ones need the filter.
    //
    // Original note: VMF.NET also exposes its own bookkeeping annotation
    // ("vmf:property:containment-info", emitted for EVERY property) through the public
    // Annotations() list, so a raw count differs from Java. That is deliberate here --
    // ShallowCopyAnnotationTests asserts the internal annotation IS visible -- so the ports
    // assert on the user-declared annotations rather than on the total count.
    private static System.Collections.Generic.List<VMF.NET.Runtime.IAnnotation> UserAnnotations(
        VMF.NET.Runtime.IVObject o, string propertyName) =>
        o.Vmf().Reflect().PropertyByName(propertyName)!.Annotations()
            .Where(a => !a.Key.StartsWith("vmf:")).ToList();

    [Fact]
    public void BasicAnnotationTest()
    {
        var annotatedModel = IAnnotatedModel.NewInstance();

        var annotations = annotatedModel.Vmf().Reflect().Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my value 2"), $"Not as expected, got: {annotations[1]}");

        var propertyAnnotations = UserAnnotations(annotatedModel, "Name");

        Assert.Equal(2, propertyAnnotations.Count);
        Assert.True(propertyAnnotations[0].Equals("prop key 1", "my prop value 1"),
            $"Not as expected, got: {propertyAnnotations[0]}");
        Assert.True(propertyAnnotations[1].Equals("prop key 2", "my prop value 2"),
            $"Not as expected, got: {propertyAnnotations[1]}");

        Assert.NotNull(annotatedModel.Vmf().Reflect().AnnotationByKey("key 1"));
        Assert.NotNull(annotatedModel.Vmf().Reflect().AnnotationByKey("key 2"));
        Assert.Null(annotatedModel.Vmf().Reflect().AnnotationByKey("key 3"));
    }

    [Fact]
    public void MultipleAnnotationsPerKeyTest()
    {
        var annotatedObject = IMultipleAnnotationsPerKey.NewInstance();

        var annotations = annotatedObject.Vmf().Reflect().Annotations();

        Assert.Equal(3, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my value 1"), $"Not as expected, got: {annotations[0]}");
        Assert.Equal("key 2", annotations[1].Key);
        Assert.Equal("key 2", annotations[2].Key);

        var key1 = annotatedObject.Vmf().Reflect().AnnotationsByKey("key 1");
        var key2 = annotatedObject.Vmf().Reflect().AnnotationsByKey("key 2");

        Assert.Single(key1);
        Assert.Equal(2, key2.Count);
    }

    [Fact]
    public void AnnotationInheritanceTest()
    {
        var annotatedObjectParent = IAnnotationInheritance1Parent.NewInstance();

        var annotations = annotatedObjectParent.Vmf().Reflect().Annotations();

        Assert.Equal(2, annotations.Count);
        Assert.True(annotations[0].Equals("key 1", "my parent value 1"),
            $"Not as expected, got: {annotations[0]}");
        Assert.True(annotations[1].Equals("key 2", "my parent value 2"),
            $"Not as expected, got: {annotations[1]}");

        var annotatedObjectChild = IAnnotationInheritance1Child.NewInstance();

        annotations = annotatedObjectChild.Vmf().Reflect().Annotations();

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
        var parent = IAnnotationInheritance2Parent.NewInstance();
        var parentAnnotations = UserAnnotations(parent, "Name");

        Assert.Equal(2, parentAnnotations.Count);
        Assert.Equal("key 1", parentAnnotations[0].Key);
        Assert.Equal("my parent value 1", parentAnnotations[0].Value);
        Assert.Equal("key 2", parentAnnotations[1].Key);
        Assert.Equal("my parent value 2", parentAnnotations[1].Value);

        var child = IAnnotationInheritance2Child.NewInstance();
        var childAnnotations = UserAnnotations(child, "Name");

        Assert.Equal(2, childAnnotations.Count);
        Assert.Equal("key 1", childAnnotations[0].Key);
        Assert.Equal("my child value 1", childAnnotations[0].Value);
        Assert.Equal("key 2", childAnnotations[1].Key);
        Assert.Equal("my child value 2", childAnnotations[1].Value);
    }
}
