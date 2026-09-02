// Regression: an [InterfaceOnly] type in the model's namespace broke schema generation entirely.
// Reported against 0.3.2 from BFXClientNETModel.

using VMF.NET.Json;
using VMF.NET.TestSuite.JSONSchemaGeneration.InterfaceOnlyCase;
using Xunit;

namespace VMF.NET.TestSuite;

public class InterfaceOnlySchemaTests
{
    [Fact]
    public void AnInterfaceOnlyTypeInTheNamespace_DoesNotBreakSchemaGeneration()
    {
        // Threw: "Cannot reflect on type '...ConfigElement' without an instance: it has no
        // prototype factory." GetSubTypes asked every type in the namespace for its SuperTypes(),
        // and an interface-only type cannot answer that -- so the mere presence of one was fatal.
        var json = new VmfJsonSchemaGenerator().GenerateSchemaAsString<ConfigHolder>();

        Assert.Contains("apps", json);
    }

    [Fact]
    public void TheInterfaceOnlyBase_IsNotOfferedAsAChoice()
    {
        // AppConfig extends the interface-only ConfigElement, so ConfigElement must never appear
        // as a oneOf alternative: it cannot be instantiated. Java removes interface-only types
        // from the choices for the same reason.
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<ConfigHolder>();
        var apps = (Dictionary<string, object>)((Dictionary<string, object>)schema["properties"])["apps"];
        var items = (Dictionary<string, object>)apps["items"];

        Assert.DoesNotContain("ConfigElement", System.Text.Json.JsonSerializer.Serialize(items));
    }

    [Fact]
    public void ARedeclaredProperty_UsesTheDerivedInterfacesAnnotations()
    {
        // Java lets a derived interface redeclare an inherited property to give it its own
        // annotations. C# needs `new` to say the hiding is intended; this pins that the DERIVED
        // declaration's annotations are the ones that reach the schema.
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<ConfigHolder>();
        var definitions = (Dictionary<string, object>)schema["definitions"];

        var piezo = (Dictionary<string, object>)
            definitions["VMF.NET.TestSuite.JSONSchemaGeneration.InterfaceOnlyCase.PiezoChannelConfig"];
        var props = (Dictionary<string, object>)piezo["properties"];
        var channelId = (Dictionary<string, object>)props["channelId"];

        Assert.Equal("Channel ID", ((System.Text.Json.JsonElement)channelId["title"]).GetString());
        Assert.Equal(0, ((System.Text.Json.JsonElement)channelId["propertyOrder"]).GetInt32());
    }
}
