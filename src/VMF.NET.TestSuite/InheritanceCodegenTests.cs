// Issue A — model-interface inheritance must generate compilable, correct implementations.
// On 0.1.3 the model file itself fails to compile (CS0738/CS0539); once fixed, these pass.

using VMF.NET.TestSuite.Models;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite;

public class InheritanceCodegenTests
{
    [Fact]
    public void Subtype_exposes_inherited_and_own_properties()
    {
        var dog = Dog.NewInstance();
        dog.Name = "Rex";   // inherited from Animal
        dog.Age = 3;        // inherited from Animal
        dog.Breed = "Lab";  // own

        Assert.Equal("Rex", dog.Name);
        Assert.Equal(3, dog.Age);
        Assert.Equal("Lab", dog.Breed);
    }

    [Fact]
    public void Subtype_is_assignable_to_base()
    {
        Animal animal = Cat.NewInstance();
        animal.Name = "Mia";

        Assert.IsAssignableFrom<Animal>(animal);
        Assert.True(animal is Cat);
        Assert.Equal("Mia", animal.Name);
    }

    [Fact]
    public void Clone_via_base_interface_preserves_concrete_type_and_state()
    {
        var dog = Dog.NewInstance();
        dog.Name = "Rex";
        dog.Age = 3;
        dog.Breed = "Lab";

        Animal asBase = dog;
        var clone = asBase.Clone();          // Animal.Clone() must be implemented on the subtype impl

        Assert.False(ReferenceEquals(dog, clone));
        Assert.True(clone is Dog, "clone of an Dog (via Animal) must still be an Dog");
        Assert.Equal("Rex", clone.Name);
        Assert.Equal("Lab", ((Dog)clone).Breed);
    }

    [Fact]
    public void AsReadOnly_via_base_interface_works()
    {
        var cat = Cat.NewInstance();
        cat.Name = "Mia";
        cat.Indoor = true;

        Animal asBase = cat;
        var readOnly = asBase.AsReadOnly();  // Animal.AsReadOnly() must be implemented on the subtype impl

        Assert.Equal("Mia", readOnly.Name);
    }

    [Fact]
    public void Containment_of_subtypes_in_base_typed_list_tracks_parent()
    {
        var zoo = Zoo.NewInstance();
        zoo.Name = "City Zoo";

        var dog = Dog.NewInstance();
        dog.Name = "Rex";
        var cat = Cat.NewInstance();
        cat.Name = "Mia";

        zoo.Animals.Add(dog);
        zoo.Animals.Add(cat);

        Assert.Equal(2, zoo.Animals.Count);
        Assert.Same(zoo, dog.Zoo);   // containment back-ref declared on the base, set from a subtype
        Assert.Same(zoo, cat.Zoo);
    }

    [Fact]
    public void Equals_and_hashcode_account_for_subtype_state()
    {
        var d1 = Dog.NewInstance(); d1.Name = "Rex"; d1.Breed = "Lab";
        var d2 = Dog.NewInstance(); d2.Name = "Rex"; d2.Breed = "Lab";
        var d3 = Dog.NewInstance(); d3.Name = "Rex"; d3.Breed = "Poodle";

        Assert.Equal(d1, d2);
        Assert.Equal(d1.GetHashCode(), d2.GetHashCode());
        Assert.NotEqual(d1, d3);
    }

    // ------------------------------------------------------------------
    // A SUPERTYPE's builder applies only that supertype's state.
    //
    // Found by completing the API-coverage audit (issue #2): ApplyFrom/ApplyTo were covered, but
    // only ever through the builder of the same type, where "copies the properties" and "copies
    // the supertype's properties" are indistinguishable. The selective behaviour is what
    // Tutorial 05 teaches, and it had no test -- which is how the tutorial's port came to be
    // flattened into a single interface, destroying the lesson, without anything noticing.
    // ------------------------------------------------------------------

    [Fact]
    public void ASupertypeBuilder_AppliesOnlyTheSupertypesProperties()
    {
        var source = Dog.NewInstance();
        source.Name = "Rex";
        source.Age = 3;
        source.Breed = "Husky";

        var target = Dog.NewInstance();
        target.Name = "Fido";
        target.Age = 9;
        target.Breed = "Poodle";

        // Animal declares Name and Age; Breed belongs to Dog alone.
        Animal.NewBuilder().ApplyFrom(source).ApplyTo(target);

        Assert.Equal("Rex", target.Name);
        Assert.Equal(3, target.Age);
        Assert.Equal("Poodle", target.Breed);   // untouched -- not part of Animal
    }

    [Fact]
    public void TheConcreteBuilder_AppliesEverything()
    {
        // The contrast that gives the test above its meaning.
        var source = Dog.NewInstance();
        source.Name = "Rex";
        source.Breed = "Husky";

        var target = Dog.NewInstance();
        target.Name = "Fido";
        target.Breed = "Poodle";

        Dog.NewBuilder().ApplyFrom(source).ApplyTo(target);

        Assert.Equal("Rex", target.Name);
        Assert.Equal("Husky", target.Breed);
    }
}
