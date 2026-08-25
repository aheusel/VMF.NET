// Repro model for Issue A (inheritance codegen) and Issue B (polymorphic JSON).
//
// A base model interface (IAnimal) with two concrete subtypes (IDog, ICat), held in a
// heterogeneous CONTAINMENT list on a container type (IZoo). Mutable variant.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

[VmfModel(Equality = EqualsType.All)]
interface IAnimal
{
    string? Name { get; set; }
    int Age { get; set; }

    // Containment back-ref (inherited by every subtype).
    [Container("IZoo.Animals")]
    IZoo? Zoo { get; }
}

[VmfModel(Equality = EqualsType.All)]
interface IDog : IAnimal
{
    string? Breed { get; set; }
}

[VmfModel(Equality = EqualsType.All)]
interface ICat : IAnimal
{
    bool Indoor { get; set; }
}

[VmfModel(Equality = EqualsType.All)]
interface IZoo
{
    string? Name { get; set; }

    // Heterogeneous list typed by the base; holds IDog / ICat instances.
    [Contains("IAnimal.Zoo")]
    IAnimal[] Animals { get; }
}
