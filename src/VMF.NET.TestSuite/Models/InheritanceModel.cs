// Repro model for Issue A (inheritance codegen) and Issue B (polymorphic JSON).
//
// A base model interface (Animal) with two concrete subtypes (Dog, Cat), held in a
// heterogeneous CONTAINMENT list on a container type (Zoo). Mutable variant.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
interface Animal
{
    string? Name { get; set; }
    int Age { get; set; }

    // Containment back-ref (inherited by every subtype).
    [Container("Zoo.Animals")]
    Zoo? Zoo { get; }
}

[VmfModel(Equality = EqualsType.All)]
interface Dog : Animal
{
    string? Breed { get; set; }
}

[VmfModel(Equality = EqualsType.All)]
interface Cat : Animal
{
    bool Indoor { get; set; }
}

[VmfModel(Equality = EqualsType.All)]
interface Zoo
{
    string? Name { get; set; }

    // Heterogeneous list typed by the base; holds Dog / Cat instances.
    [Contains("Animal.Zoo")]
    Animal[] Animals { get; }
}
