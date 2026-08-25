// Ported from eu.mihosoft.vmftest.ignoretostring.vmfmodel.SampleClass

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.IgnoreToString.VmfModel;

interface ISampleClass
{
    string? Name { get; set; }

    [IgnoreToString]
    string? IgnoredProp { get; set; }
}
