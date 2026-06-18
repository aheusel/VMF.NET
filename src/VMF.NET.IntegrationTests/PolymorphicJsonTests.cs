// Issue A + B — a heterogeneous list of subtypes must serialize with a @vmf-type
// discriminator and deserialize back to the concrete subtypes (mutable + immutable paths).
// This is the core acceptance for the polymorphic-document use case.

using System.Text.Json;
using VMF.NET.IntegrationTests.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

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
        var zoo = IZoo.NewInstance();
        zoo.Name = "City Zoo";

        var dog = IDog.NewInstance(); dog.Name = "Rex"; dog.Age = 3; dog.Breed = "Lab";
        var cat = ICat.NewInstance(); cat.Name = "Mia"; cat.Age = 2; cat.Indoor = true;
        zoo.Animals.Add(dog);
        zoo.Animals.Add(cat);

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(zoo, options);

        // The declared element type is IAnimal but the runtime types differ → discriminator required.
        Assert.Contains("@vmf-type", json);

        var restored = JsonSerializer.Deserialize<IZoo>(json, options)!;

        Assert.Equal(2, restored.Animals.Count);
        Assert.True(restored.Animals[0] is IDog, "Animals[0] must round-trip as IDog, not the base IAnimal");
        Assert.True(restored.Animals[1] is ICat, "Animals[1] must round-trip as ICat, not the base IAnimal");

        // subtype-specific state preserved
        Assert.Equal("Lab", ((IDog)restored.Animals[0]).Breed);
        Assert.True(((ICat)restored.Animals[1]).Indoor);

        // inherited state preserved
        Assert.Equal("Rex", restored.Animals[0].Name);
        Assert.Equal(3, restored.Animals[0].Age);
    }

    [Fact]
    public void Immutable_heterogeneous_value_list_round_trips_concrete_types()
    {
        var drawing = IDrawing.NewBuilder()
            .WithTitle("sketch")
            .WithShapes(
                ICircle.NewBuilder().WithLabel("c1").WithRadius(2.5).Build(),
                IRectangle.NewBuilder().WithLabel("r1").WithWidth(3.0).WithHeight(4.0).Build())
            .Build();

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(drawing, options);

        Assert.Contains("@vmf-type", json);

        var restored = JsonSerializer.Deserialize<IDrawing>(json, options)!;

        Assert.Equal(2, restored.Shapes.Count);
        Assert.True(restored.Shapes[0] is ICircle, "Shapes[0] must round-trip as ICircle");
        Assert.True(restored.Shapes[1] is IRectangle, "Shapes[1] must round-trip as IRectangle");

        Assert.Equal(2.5, ((ICircle)restored.Shapes[0]).Radius);
        Assert.Equal(4.0, ((IRectangle)restored.Shapes[1]).Height);
        Assert.Equal("c1", restored.Shapes[0].Label);   // inherited from IShape
    }
}
