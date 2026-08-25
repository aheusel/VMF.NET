// Issue C — nullable value-type properties (double?/int?/bool?) must compile and round-trip.
// On 0.1.3 the model file fails to compile (CS0723/0721/0722); once fixed, these pass.

using System.Text.Json;
using VMF.NET.TestSuite.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.TestSuite;

public class NullableValueTypeTests
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
    public void Nullable_value_properties_can_be_set_and_read()
    {
        var m = Measurement.NewInstance();
        m.Label = "temp";
        m.Value = 21.5;
        m.Count = null;     // explicitly unset

        Assert.Equal(21.5, m.Value!.Value);
        Assert.Null(m.Count);
    }

    [Fact]
    public void Set_nullable_values_round_trip()
    {
        var m = Measurement.NewInstance();
        m.Label = "temp";
        m.Value = 21.5;
        m.Count = 7;
        m.Flag = true;

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(m, options);
        var restored = JsonSerializer.Deserialize<Measurement>(json, options)!;

        Assert.Equal(21.5, restored.Value!.Value);
        Assert.Equal(7, restored.Count!.Value);
        Assert.True(restored.Flag!.Value);
    }

    [Fact]
    public void Null_value_properties_are_omitted_and_read_back_as_null()
    {
        var m = Measurement.NewInstance();
        m.Label = "temp";   // Value / Count / Flag left null

        var options = Options();
        var json = JsonSerializer.Serialize<IVObject>(m, options);

        using (var doc = JsonDocument.Parse(json))
        {
            // null value-type properties are omitted, matching reference-type null handling
            Assert.False(doc.RootElement.TryGetProperty("value", out _));
            Assert.False(doc.RootElement.TryGetProperty("count", out _));
            Assert.False(doc.RootElement.TryGetProperty("flag", out _));
        }

        var restored = JsonSerializer.Deserialize<Measurement>(json, options)!;
        Assert.Null(restored.Value);
        Assert.Null(restored.Count);
        Assert.Null(restored.Flag);
    }
}
