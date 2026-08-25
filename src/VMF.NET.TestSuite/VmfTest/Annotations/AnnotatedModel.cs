// Ported from eu.mihosoft.vmftest.annotations.vmfmodel.AnnotatedModel

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Annotations.VmfModel;

[VmfAnnotation("my value 1", Key = "key 1")]
[VmfAnnotation("my value 2", Key = "key 2")]
interface IAnnotatedModel
{
    [VmfAnnotation("my prop value 1", Key = "prop key 1")]
    [VmfAnnotation("my prop value 2", Key = "prop key 2")]
    string? Name { get; set; }
}

[VmfAnnotation("my value 1", Key = "key 1")]
[VmfAnnotation("my value 2", Key = "key 2")]
[VmfAnnotation("my value 3", Key = "key 2")]
interface IMultipleAnnotationsPerKey
{
}

[VmfAnnotation("my parent value 1", Key = "key 1")]
[VmfAnnotation("my parent value 2", Key = "key 2")]
interface IAnnotationInheritance1Parent
{
}

[VmfAnnotation("my child value 1", Key = "key 1")]
[VmfAnnotation("my child value 2", Key = "key 2")]
interface IAnnotationInheritance1Child
{
}

interface IAnnotationInheritance2Parent
{
    [VmfAnnotation("my parent value 1", Key = "key 1")]
    [VmfAnnotation("my parent value 2", Key = "key 2")]
    string? Name { get; set; }
}

interface IAnnotationInheritance2Child
{
    [VmfAnnotation("my child value 1", Key = "key 1")]
    [VmfAnnotation("my child value 2", Key = "key 2")]
    string? Name { get; set; }
}
