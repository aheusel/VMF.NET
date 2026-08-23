// Ported from eu.mihosoft.vmftest.annotations.vmfmodel.AnnotatedModel

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Annotations;

[VmfModel]
[VmfAnnotation("my value 1", Key = "key 1")]
[VmfAnnotation("my value 2", Key = "key 2")]
public partial interface IAnnotatedModel
{
    [VmfAnnotation("my prop value 1", Key = "prop key 1")]
    [VmfAnnotation("my prop value 2", Key = "prop key 2")]
    string? Name { get; set; }
}

[VmfModel]
[VmfAnnotation("my value 1", Key = "key 1")]
[VmfAnnotation("my value 2", Key = "key 2")]
[VmfAnnotation("my value 3", Key = "key 2")]
public partial interface IMultipleAnnotationsPerKey
{
}

[VmfModel]
[VmfAnnotation("my parent value 1", Key = "key 1")]
[VmfAnnotation("my parent value 2", Key = "key 2")]
public partial interface IAnnotationInheritance1Parent
{
}

[VmfModel]
[VmfAnnotation("my child value 1", Key = "key 1")]
[VmfAnnotation("my child value 2", Key = "key 2")]
public partial interface IAnnotationInheritance1Child
{
}

[VmfModel]
public partial interface IAnnotationInheritance2Parent
{
    [VmfAnnotation("my parent value 1", Key = "key 1")]
    [VmfAnnotation("my parent value 2", Key = "key 2")]
    string? Name { get; set; }
}

[VmfModel]
public partial interface IAnnotationInheritance2Child
{
    [VmfAnnotation("my child value 1", Key = "key 1")]
    [VmfAnnotation("my child value 2", Key = "key 2")]
    string? Name { get; set; }
}
