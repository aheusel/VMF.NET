// Repro model for parameterless [Contains] — containment WITHOUT a declared opposite.
//
// A container (Box) owns a heterogeneous-free list of children (BoxItem) via [Contains] with no
// opposite argument. The contained type declares NO [Container] back-reference: its parent link is
// tracked internally by the generated implementation. Mutable model (immutable types cannot
// participate in containment).

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
interface BoxItem
{
    string? Name { get; set; }
    // No [Container] property — the parent is not part of the contained type's public surface.
}

[VmfModel(Equality = EqualsType.All)]
interface Box
{
    string? Label { get; set; }

    // Parameterless [Contains]: opposite-less containment. Requires a parameterless ContainsAttribute
    // constructor (the fix); the generator's existing "without opposite" path handles the rest.
    [Contains]
    BoxItem[] Items { get; }
}
