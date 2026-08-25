// Issue A + B — a heterogeneous list of subtypes must serialize with a @vmf-type
// discriminator and deserialize back to the concrete subtypes (mutable + immutable paths).
// This is the core acceptance for the polymorphic-document use case.

using System.Text.Json;
using VMF.NET.TestSuite.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite;

public class PolymorphicJsonTests
{
    private static JsonSerializerOptions Options()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new VmfJsonConverterFactory());
        return options;
    }

    [Fact]
    public void Mutable_heterogeneous_containment_list_round_trips_concrete_types()
    {
        var zoo = Zoo.NewInstance();
        zoo.Name = "City Zoo";

        var dog = Dog.NewInstance(); dog.Name = "Rex"; dog.Age = 3; dog.Breed = "Lab";
        var cat = Cat.NewInstance(); cat.Name = "Mia"; cat.Age = 2; cat.Indoor = true;
        zoo.Animals.Add(dog);
        zoo.Animals.Add(cat);

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(zoo, options);

        // The declared element type is Animal but the runtime types differ → discriminator required.
        Assert.Contains("@vmf-type", json);

        var restored = JsonSerializer.Deserialize<Zoo>(json, options)!;

        Assert.Equal(2, restored.Animals.Count);
        Assert.True(restored.Animals[0] is Dog, "Animals[0] must round-trip as Dog, not the base Animal");
        Assert.True(restored.Animals[1] is Cat, "Animals[1] must round-trip as Cat, not the base Animal");

        // subtype-specific state preserved
        Assert.Equal("Lab", ((Dog)restored.Animals[0]).Breed);
        Assert.True(((Cat)restored.Animals[1]).Indoor);

        // inherited state preserved
        Assert.Equal("Rex", restored.Animals[0].Name);
        Assert.Equal(3, restored.Animals[0].Age);
    }

    [Fact]
    public void Immutable_heterogeneous_value_list_round_trips_concrete_types()
    {
        var drawing = Drawing.NewBuilder()
            .WithTitle("sketch")
            .WithShapes(
                Circle.NewBuilder().WithLabel("c1").WithRadius(2.5).Build(),
                Rectangle.NewBuilder().WithLabel("r1").WithWidth(3.0).WithHeight(4.0).Build())
            .Build();

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(drawing, options);

        Assert.Contains("@vmf-type", json);

        var restored = JsonSerializer.Deserialize<Drawing>(json, options)!;

        Assert.Equal(2, restored.Shapes.Count);
        Assert.True(restored.Shapes[0] is Circle, "Shapes[0] must round-trip as Circle");
        Assert.True(restored.Shapes[1] is Rectangle, "Shapes[1] must round-trip as Rectangle");

        Assert.Equal(2.5, ((Circle)restored.Shapes[0]).Radius);
        Assert.Equal(4.0, ((Rectangle)restored.Shapes[1]).Height);
        Assert.Equal("c1", restored.Shapes[0].Label);   // inherited from Shape
    }
}
