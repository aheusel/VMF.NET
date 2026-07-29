// Finding 4 Step B — schema-annotation value validation + JSON-array defaults.
// Previously malformed vmf:schema:* values were silently dropped (or a default=[...] rendered as
// a string); now the generator parses array/object values as real JSON and fails fast with an
// actionable VmfSchemaAnnotationException on a malformed value.

using System.Text.Json;
using VMF.NET.IntegrationTests.Models;
using VMF.NET.Json;
using Xunit;

namespace VMF.NET.IntegrationTests;

public class SchemaValidationTests
{
    private static Dictionary<string, object> PropSchema<T>(string prop) where T : VMF.NET.Runtime.IVObject
    {
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<T>();
        var properties = (Dictionary<string, object>)schema["properties"];
        return (Dictionary<string, object>)properties[prop];
    }

    // --- valid cases ---

    [Fact]
    public void ArrayDefault_RendersAsJsonArray_NotString()
    {
        var deviceIds = PropSchema<IArrayDefaultConfig>("DeviceIds");

        var def = Assert.IsType<JsonElement>(deviceIds["default"]);
        Assert.Equal(JsonValueKind.Array, def.ValueKind);
        Assert.Equal(3, def.GetArrayLength());
        Assert.Equal("alldrop", def[0].GetString());
        Assert.Equal("biospot", def[2].GetString());
    }

    [Fact]
    public void ValidPattern_IsAccepted()
    {
        var code = PropSchema<IValidPatternConfig>("Code");
        Assert.Equal("^\\d{3}$", code["pattern"]);
    }

    [Fact]
    public void UnknownScalarKeyword_IsAcceptedVerbatim()
    {
        // The constraint catch-all stays open-ended: unknown keywords are not rejected.
        var legacy = PropSchema<IUnknownKeywordConfig>("Legacy");
        Assert.Equal(true, legacy["deprecated"]);
    }

    // --- malformed cases: each throws a descriptive exception instead of silently dropping ---

    [Fact]
    public void NonNumericMinimum_Throws()
    {
        var ex = Assert.Throws<VmfSchemaAnnotationException>(
            () => new VmfJsonSchemaGenerator().GenerateSchema<IBadMinimumConfig>());
        Assert.Contains("minimum", ex.Message);
        Assert.Contains("numeric", ex.Message);
    }

    [Fact]
    public void UncompilablePattern_Throws()
    {
        var ex = Assert.Throws<VmfSchemaAnnotationException>(
            () => new VmfJsonSchemaGenerator().GenerateSchema<IBadPatternConfig>());
        Assert.Contains("pattern", ex.Message);
    }

    [Fact]
    public void ConstraintWithoutEquals_Throws()
    {
        var ex = Assert.Throws<VmfSchemaAnnotationException>(
            () => new VmfJsonSchemaGenerator().GenerateSchema<IBadConstraintFormConfig>());
        Assert.Contains("keyword=value", ex.Message);
    }

    [Fact]
    public void NonBooleanUniqueItems_Throws()
    {
        var ex = Assert.Throws<VmfSchemaAnnotationException>(
            () => new VmfJsonSchemaGenerator().GenerateSchema<IBadUniqueItemsConfig>());
        Assert.Contains("uniqueItems", ex.Message);
    }

    [Fact]
    public void MalformedInjectJson_Throws()
    {
        var ex = Assert.Throws<VmfSchemaAnnotationException>(
            () => new VmfJsonSchemaGenerator().GenerateSchema<IBadInjectConfig>());
        Assert.Contains("valid JSON", ex.Message);
    }
}
