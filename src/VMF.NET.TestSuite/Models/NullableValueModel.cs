// Repro model for Issue C: nullable value-type properties (double?/int?/bool?).
// On 0.1.3 the generated implementation fails to compile (CS0723/0721/0722).

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
interface IMeasurement
{
    string? Label { get; set; }
    double? Value { get; set; }
    int? Count { get; set; }
    bool? Flag { get; set; }
}
