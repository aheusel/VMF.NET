// Ported from eu.mihosoft.vmftests.completepropertyordertest.vmfmodel
//
// Only the VALID model is ported here. Java's IncompleteOrderInfo (partial @PropertyOrder)
// and InvalidOrderInfo (duplicate index) are negative models: VMF must reject them. In
// VMF.NET a model error is reported as a build error, so they cannot live in a compiled
// project -- they belong in a compile-gate test in VMF.NET.Tests, where the model is passed
// as source text and the diagnostic is asserted.

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.CompletePropertyOrderTest;

[VmfModel]
public partial interface ICompleteOrderInfo
{
    [PropertyOrder(1)] string? A { get; set; }
    [PropertyOrder(2)] string? B { get; set; }
}
