// Regression tests for the polymorphism check behind the "@vmf-type" discriminator.
//
// VmfTypeUtils.IsPolymorphic asks whether any SUPERTYPE of the serialised object's type is used
// as a property type anywhere in the model. Answering that needs the properties of every model
// type, which before M5 could not be obtained without an instance -- so the check only ever
// looked at the object's own type, and silently answered "no" whenever the supertype was used
// on some other type.

using System.Text.Json;
using VMF.NET.Json;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.Models;
using Xunit;

namespace VMF.NET.TestSuite;

public class PolymorphicDiscriminatorTests
{
    private static JsonSerializerOptions Options()
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        options.Converters.Add(new VmfJsonConverterFactory());
        return options;
    }

    [Fact]
    public void Subtype_SerialisedStandalone_StillCarriesTheDiscriminator()
    {
        // ICircle's supertype IShape is used as a property type on IDrawing -- a DIFFERENT type.
        // Serialising a circle on its own must still say which subtype it is, or the value
        // cannot be read back into an IShape-typed slot.
        var circle = ICircle.NewBuilder().WithLabel("c1").WithRadius(2.0).Build();

        var json = JsonSerializer.Serialize(circle, Options());

        Assert.Contains("@vmf-type", json);
    }

    [Fact]
    public void Subtype_InsideItsPolymorphicContainer_CarriesTheDiscriminator()
    {
        // The case that worked before: the supertype is used by the very object being written.
        var drawing = IDrawing.NewBuilder()
            .WithTitle("d1")
            .WithShapes(ICircle.NewBuilder().WithLabel("c1").WithRadius(2.0).Build())
            .Build();

        var json = JsonSerializer.Serialize(drawing, Options());

        Assert.Contains("@vmf-type", json);
    }

    [Fact]
    public void TypeWithNoSubtypeRelationship_CarriesNoDiscriminator()
    {
        // The check must stay a check: a type nothing uses polymorphically gets no discriminator.
        var drawing = IDrawing.NewBuilder().WithTitle("d1").Build();

        var json = JsonSerializer.Serialize(drawing, Options());

        Assert.DoesNotContain("@vmf-type", json);
    }
}
