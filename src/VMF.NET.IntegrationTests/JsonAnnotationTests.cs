using System.Text.Json;
using VMF.NET.IntegrationTests.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

public class JsonAnnotationTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new VmfJsonConverterFactory());
        return options;
    }

    // --- Rename annotation ---

    [Fact]
    public void Serialize_Rename_UsesAnnotatedFieldName()
    {
        var config = IServiceConfig.NewInstance();
        config.Name = "MyService";

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(config, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // "Name" should be serialized as "service_name" per the rename annotation
        Assert.True(root.TryGetProperty("service_name", out var nameElem));
        Assert.Equal("MyService", nameElem.GetString());
        // Original property name should not appear
        Assert.False(root.TryGetProperty("name", out _));
        Assert.False(root.TryGetProperty("Name", out _));
    }

    [Fact]
    public void Deserialize_Rename_ReadsAnnotatedFieldName()
    {
        var json = """{"service_name":"MyService","port":9090}""";

        var options = CreateOptions();
        var config = JsonSerializer.Deserialize<IServiceConfig>(json, options)!;

        Assert.Equal("MyService", config.Name);
        Assert.Equal(9090, config.Port);
    }

    [Fact]
    public void RoundTrip_Rename()
    {
        var original = IServiceConfig.NewInstance();
        original.Name = "RoundTrip";
        original.Port = 3000;

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(original, options);
        var deserialized = JsonSerializer.Deserialize<IServiceConfig>(json, options)!;

        Assert.Equal("RoundTrip", deserialized.Name);
        Assert.Equal(3000, deserialized.Port);
    }

    // --- JSON Schema annotations ---

    [Fact]
    public void Schema_Description()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var portSchema = (Dictionary<string, object>)properties["Port"];
        Assert.Equal("The port number for the service", portSchema["description"]);
    }

    [Fact]
    public void Schema_Constraints()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var portSchema = (Dictionary<string, object>)properties["Port"];
        Assert.Equal(1, portSchema["minimum"]);
        Assert.Equal(65535, portSchema["maximum"]);
    }

    [Fact]
    public void Schema_Format()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var hostSchema = (Dictionary<string, object>)properties["Host"];
        Assert.Equal("hostname", hostSchema["format"]);
    }

    [Fact]
    public void Schema_Title()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var hostSchema = (Dictionary<string, object>)properties["Host"];
        Assert.Equal("Server Hostname", hostSchema["title"]);
    }

    [Fact]
    public void Schema_UniqueItems()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var tagsSchema = (Dictionary<string, object>)properties["Tags"];
        Assert.Equal(true, tagsSchema["uniqueItems"]);
    }

    [Fact]
    public void Schema_PropertyOrder()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var enabledSchema = (Dictionary<string, object>)properties["Enabled"];
        Assert.Equal(1, enabledSchema["propertyOrder"]);
    }

    [Fact]
    public void Schema_DefaultValue()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        var portSchema = (Dictionary<string, object>)properties["Port"];
        Assert.Equal(8080, portSchema["default"]);
    }

    [Fact]
    public void Schema_RenamedField()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<IServiceConfig>();

        var properties = (Dictionary<string, object>)schema["properties"];
        // Name should appear under its renamed field name "service_name"
        Assert.True(properties.ContainsKey("service_name"));
        Assert.False(properties.ContainsKey("Name"));
    }
}
