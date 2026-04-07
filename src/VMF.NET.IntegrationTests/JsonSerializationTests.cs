using System.Text.Json;
using VMF.NET.IntegrationTests.Models;
using VMF.NET.Json;
using VMF.NET.Runtime;
using Xunit;

namespace VMF.NET.IntegrationTests;

public class JsonSerializationTests
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

    [Fact]
    public void Serialize_SimpleProperties()
    {
        var node = INode.NewInstance();
        node.Name = "Start";
        node.X = 10;
        node.Y = 20;

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(node, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Start", root.GetProperty("name").GetString());
        Assert.Equal(10, root.GetProperty("x").GetInt32());
        Assert.Equal(20, root.GetProperty("y").GetInt32());
    }

    [Fact]
    public void Serialize_SkipsContainerAndCrossRef()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "MyFlow";

        var node = INode.NewInstance();
        node.Name = "N1";
        flow.Nodes.Add(node);

        var conn = IConnection.NewInstance();
        conn.Sender = node;
        flow.Connections.Add(conn);

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(flow, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Flow should have title, nodes, connections (contained)
        Assert.Equal("MyFlow", root.GetProperty("title").GetString());
        Assert.True(root.TryGetProperty("nodes", out var nodesElem));
        Assert.Equal(1, nodesElem.GetArrayLength());

        // Node in the serialized output should NOT have "flow" (container) or "outputs"/"inputs" (cross-refs)
        var nodeElem = nodesElem[0];
        Assert.Equal("N1", nodeElem.GetProperty("name").GetString());
        Assert.False(nodeElem.TryGetProperty("flow", out _));
        Assert.False(nodeElem.TryGetProperty("outputs", out _));
        Assert.False(nodeElem.TryGetProperty("inputs", out _));
    }

    [Fact]
    public void Serialize_ImmutableType()
    {
        var point = IPoint.NewBuilder().WithX(3.14).WithY(2.72).Build();

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(point, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(3.14, root.GetProperty("x").GetDouble());
        Assert.Equal(2.72, root.GetProperty("y").GetDouble());
    }

    [Fact]
    public void Deserialize_SimpleProperties()
    {
        var json = """{"name":"Start","x":10,"y":20}""";

        var options = CreateOptions();
        var node = JsonSerializer.Deserialize<INode>(json, options)!;

        Assert.Equal("Start", node.Name);
        Assert.Equal(10, node.X);
        Assert.Equal(20, node.Y);
    }

    [Fact]
    public void Deserialize_ImmutableType()
    {
        var json = """{"x":3.14,"y":2.72}""";

        var options = CreateOptions();
        var point = JsonSerializer.Deserialize<IPoint>(json, options)!;

        Assert.Equal(3.14, point.X);
        Assert.Equal(2.72, point.Y);
    }

    [Fact]
    public void RoundTrip_SimpleObject()
    {
        var original = INode.NewInstance();
        original.Name = "Test";
        original.X = 42;
        original.Y = 99;

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(original, options);
        var deserialized = JsonSerializer.Deserialize<INode>(json, options)!;

        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
    }

    [Fact]
    public void RoundTrip_WithContainment()
    {
        var flow = IFlow.NewInstance();
        flow.Title = "RoundTrip";

        var n1 = INode.NewInstance();
        n1.Name = "A";
        n1.X = 1;
        var n2 = INode.NewInstance();
        n2.Name = "B";
        n2.X = 2;
        flow.Nodes.Add(n1);
        flow.Nodes.Add(n2);

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(flow, options);
        var deserialized = JsonSerializer.Deserialize<IFlow>(json, options)!;

        Assert.Equal("RoundTrip", deserialized.Title);
        Assert.Equal(2, deserialized.Nodes.Count);
        Assert.Equal("A", deserialized.Nodes[0].Name);
        Assert.Equal("B", deserialized.Nodes[1].Name);
        // Containment should be restored
        Assert.Same(deserialized, deserialized.Nodes[0].Flow);
    }

    [Fact]
    public void RoundTrip_ImmutableCollection()
    {
        var shape = IShape.NewBuilder()
            .WithName("Triangle")
            .WithPoints(
                IPoint.NewBuilder().WithX(0).WithY(0).Build(),
                IPoint.NewBuilder().WithX(1).WithY(0).Build(),
                IPoint.NewBuilder().WithX(0).WithY(1).Build()
            )
            .Build();

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(shape, options);
        var deserialized = JsonSerializer.Deserialize<IShape>(json, options)!;

        Assert.Equal("Triangle", deserialized.Name);
        Assert.Equal(3, deserialized.Points.Count);
        Assert.Equal(1.0, deserialized.Points[1].X);
    }

    [Fact]
    public void Deserialize_IgnoresUnknownProperties()
    {
        var json = """{"name":"Test","unknownField":"ignored","x":5,"y":10}""";

        var options = CreateOptions();
        var node = JsonSerializer.Deserialize<INode>(json, options)!;

        Assert.Equal("Test", node.Name);
        Assert.Equal(5, node.X);
    }

    [Fact]
    public void Serialize_NullValues_Omitted()
    {
        var node = INode.NewInstance();
        // Name is null by default

        var options = CreateOptions();
        var json = JsonSerializer.Serialize<IVObject>(node, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // null values should be omitted from output
        Assert.False(root.TryGetProperty("name", out _));
    }

    [Fact]
    public void JsonSchema_ContainsProperties()
    {
        var generator = new VmfJsonSchemaGenerator();
        var schema = generator.GenerateSchema<INode>();

        Assert.Equal("http://json-schema.org/draft-07/schema#", schema["$schema"]);
        Assert.Equal("object", schema["type"]);

        var properties = (Dictionary<string, object>)schema["properties"];
        Assert.True(properties.ContainsKey("Name"));
        Assert.True(properties.ContainsKey("X"));
        Assert.True(properties.ContainsKey("Y"));
        // Container property (Flow) should be excluded
        Assert.False(properties.ContainsKey("Flow"));
    }

    [Fact]
    public void JsonSchema_AsString()
    {
        var generator = new VmfJsonSchemaGenerator();
        var json = generator.GenerateSchemaAsString<IPoint>();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("http://json-schema.org/draft-07/schema#", root.GetProperty("$schema").GetString());
        Assert.True(root.TryGetProperty("properties", out var props));
        Assert.True(props.TryGetProperty("X", out _));
        Assert.True(props.TryGetProperty("Y", out _));
    }
}
