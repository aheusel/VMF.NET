// JSON Schema generation: polymorphism, and the naming rule shared with the serializer.
//
// Both were parity gaps against Java's VMFJsonSchemaGenerator, found while writing the JSON
// tutorial. Java emits oneOf over subtypes with @vmf-type required; VMF.NET emitted a bare $ref
// and produced schemas its own serializer's documents could not satisfy.

using System.Text.Json;
using VMF.NET.Json;
using VMF.NET.Runtime;
using VMF.NET.TestSuite.JSONSchemaGeneration;
using Xunit;

namespace VMF.NET.TestSuite;

public class SchemaGenerationTests
{
    private static Dictionary<string, object> Properties(Dictionary<string, object> schema)
        => (Dictionary<string, object>)schema["properties"];

    private static Dictionary<string, object> Definitions(Dictionary<string, object> schema)
        => (Dictionary<string, object>)schema["definitions"];

    // ------------------------------------------------------------------
    // Polymorphism
    // ------------------------------------------------------------------

    [Fact]
    public void PolymorphicList_EmitsOneOfOverSubtypesAndTheDeclaredType()
    {
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<MyModel>();

        var persons = (Dictionary<string, object>)Properties(schema)["persons"];
        Assert.Equal("array", persons["type"]);

        var items = (Dictionary<string, object>)persons["items"];
        var alternatives = Assert.IsType<List<object>>(items["oneOf"]);

        // the declared type AND its subtype, as Java's getSubTypes(elementType) + elementType
        var refs = alternatives
            .Cast<Dictionary<string, object>>()
            .Select(a => (string)a["$ref"])
            .ToList();

        Assert.Contains("#/definitions/VMF.NET.TestSuite.JSONSchemaGeneration.Person", refs);
        Assert.Contains("#/definitions/VMF.NET.TestSuite.JSONSchemaGeneration.Employee", refs);
        Assert.Equal(2, refs.Count);

        // a bare $ref would not describe the subtypes at all
        Assert.False(items.ContainsKey("$ref"));
    }

    [Fact]
    public void EachAlternative_PinsItsOwnDiscriminatorAndRequiresIt()
    {
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<MyModel>();
        var items = (Dictionary<string, object>)
            ((Dictionary<string, object>)Properties(schema)["persons"])["items"];
        var alternatives = ((List<object>)items["oneOf"]).Cast<Dictionary<string, object>>();

        foreach (var alternative in alternatives)
        {
            var typeName = ((string)alternative["$ref"]).Replace("#/definitions/", "");

            var required = Assert.IsType<string[]>(alternative["required"]);
            Assert.Equal(new[] { "@vmf-type" }, required);

            var props = (Dictionary<string, object>)alternative["properties"];
            var discriminator = (Dictionary<string, object>)props["@vmf-type"];

            Assert.Equal("string", discriminator["type"]);
            Assert.Equal(true, discriminator["readOnly"]);

            // The enum pins this alternative to exactly one type -- that is what lets a
            // validator choose between the branches.
            Assert.Equal(new[] { typeName }, Assert.IsType<string[]>(discriminator["enum"]));
        }
    }

    [Fact]
    public void Subtypes_GetTheirOwnDefinitions()
    {
        // A subtype is reachable only through its base's oneOf, never through a property, so the
        // property walk alone would leave these $refs dangling.
        var definitions = Definitions(new VmfJsonSchemaGenerator().GenerateSchema<MyModel>());

        Assert.True(definitions.ContainsKey("VMF.NET.TestSuite.JSONSchemaGeneration.Person"));
        Assert.True(definitions.ContainsKey("VMF.NET.TestSuite.JSONSchemaGeneration.Employee"));

        // and the subtype definition carries its own property, not just the inherited ones
        var employee = (Dictionary<string, object>)
            definitions["VMF.NET.TestSuite.JSONSchemaGeneration.Employee"];
        var employeeProps = (Dictionary<string, object>)employee["properties"];

        Assert.True(employeeProps.ContainsKey("employeeId"));
        Assert.True(employeeProps.ContainsKey("name"));
    }

    [Fact]
    public void NonPolymorphicReference_StaysAPlainRef()
    {
        // Address has no subtypes, so oneOf would be noise.
        var schema = new VmfJsonSchemaGenerator().GenerateSchema<Person>();
        var address = (Dictionary<string, object>)Properties(schema)["address"];

        Assert.Equal("#/definitions/VMF.NET.TestSuite.JSONSchemaGeneration.Address", address["$ref"]);
        Assert.False(address.ContainsKey("oneOf"));
    }

    [Fact]
    public void TheDiscriminatorValue_IsWhatTheSerializerActuallyWrites()
    {
        // The point of the whole exercise: a document the serializer produces must be describable
        // by the schema the generator produces.
        var model = MyModel.NewInstance();
        var employee = Employee.NewInstance();
        employee.Name = "Jane";
        model.Persons.Add(employee);

        var options = new JsonSerializerOptions { Converters = { new VmfJsonConverterFactory() } };
        var json = JsonSerializer.Serialize<IVObject>(model, options);

        using var doc = JsonDocument.Parse(json);
        var written = doc.RootElement.GetProperty("persons")[0].GetProperty("@vmf-type").GetString();

        var items = (Dictionary<string, object>)
            ((Dictionary<string, object>)Properties(
                new VmfJsonSchemaGenerator().GenerateSchema<MyModel>())["persons"])["items"];

        var allowed = ((List<object>)items["oneOf"])
            .Cast<Dictionary<string, object>>()
            .SelectMany(a => (string[])((Dictionary<string, object>)
                ((Dictionary<string, object>)a["properties"])["@vmf-type"])["enum"])
            .ToList();

        Assert.Contains(written, allowed);
    }

    // ------------------------------------------------------------------
    // Naming
    // ------------------------------------------------------------------

    [Fact]
    public void DefaultNaming_MatchesJava()
    {
        // Java's field name is the model property name -- getName() gives `name` -- and no
        // naming strategy is applied on top. C# property names are PascalCase, so the default
        // has to camelCase them to produce the same document.
        var properties = Properties(new VmfJsonSchemaGenerator().GenerateSchema<Person>());

        Assert.True(properties.ContainsKey("name"));
        Assert.True(properties.ContainsKey("age"));
        Assert.False(properties.ContainsKey("Name"));
    }

    [Fact]
    public void SchemaAndSerializer_AgreeWithoutBeingConfigured()
    {
        // The regression this whole change exists to prevent: a schema whose property names do
        // not match the documents the serializer emits.
        var person = Person.NewInstance();
        person.Name = "Ada";
        person.Age = 36;

        var options = new JsonSerializerOptions { Converters = { new VmfJsonConverterFactory() } };
        var json = JsonSerializer.Serialize<IVObject>(person, options);

        var properties = Properties(new VmfJsonSchemaGenerator().GenerateSchema<Person>());

        using var doc = JsonDocument.Parse(json);
        foreach (var field in doc.RootElement.EnumerateObject())
        {
            if (field.Name == "@vmf-type") continue;
            Assert.True(properties.ContainsKey(field.Name),
                $"serializer wrote '{field.Name}', which the schema does not describe");
        }
    }

    [Fact]
    public void WithNamingPolicy_OverridesTheDefault()
    {
        var properties = Properties(
            new VmfJsonSchemaGenerator()
                .WithNamingPolicy(JsonNamingPolicy.SnakeCaseLower)
                .GenerateSchema<ServiceEndpoint>());

        Assert.True(properties.ContainsKey("host_name"));
        Assert.False(properties.ContainsKey("hostName"));
    }

    [Fact]
    public void ARename_IsVerbatimUnderEveryPolicy()
    {
        // Java's getFieldNameForProperty returns the annotation value directly, so a rename is
        // the one field name that never moves.
        foreach (var policy in new[] { JsonNamingPolicy.CamelCase, JsonNamingPolicy.SnakeCaseLower })
        {
            var properties = Properties(
                new VmfJsonSchemaGenerator().WithNamingPolicy(policy).GenerateSchema<ServiceEndpoint>());

            Assert.True(properties.ContainsKey("service_name"));
        }

        // and the serializer agrees, whatever policy it is given
        var endpoint = ServiceEndpoint.NewInstance();
        endpoint.Name = "billing";

        foreach (var policy in new JsonNamingPolicy?[] { null, JsonNamingPolicy.SnakeCaseLower })
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = policy,
                Converters = { new VmfJsonConverterFactory() }
            };

            using var doc = JsonDocument.Parse(JsonSerializer.Serialize<IVObject>(endpoint, options));
            Assert.True(doc.RootElement.TryGetProperty("service_name", out var name));
            Assert.Equal("billing", name.GetString());
        }
    }

    [Fact]
    public void ARenamedField_StillDeserializes()
    {
        var endpoint = ServiceEndpoint.NewInstance();
        endpoint.Name = "billing";
        endpoint.HostName = "example.org";

        var options = new JsonSerializerOptions { Converters = { new VmfJsonConverterFactory() } };
        var json = JsonSerializer.Serialize<IVObject>(endpoint, options);

        var back = JsonSerializer.Deserialize<ServiceEndpoint>(json, options)!;

        Assert.Equal("billing", back.Name);
        Assert.Equal("example.org", back.HostName);
    }

    // ------------------------------------------------------------------
    // Keywords carried over from the Java model
    // ------------------------------------------------------------------

    [Fact]
    public void MultipleConstraintsOnOneProperty_AllApply()
    {
        // Java's model stacks minimum=0 and maximum=99 on `age`; both must survive.
        var age = (Dictionary<string, object>)
            Properties(new VmfJsonSchemaGenerator().GenerateSchema<Person>())["age"];

        Assert.Equal("integer", age["type"]);
        Assert.Equal(30, age["default"]);
        Assert.Equal(0, age["minimum"]);
        Assert.Equal(99, age["maximum"]);
    }

    [Fact]
    public void DescriptionAnnotation_ReachesTheSchema()
    {
        var address = (Dictionary<string, object>)
            Properties(new VmfJsonSchemaGenerator().GenerateSchema<Person>())["address"];

        Assert.Equal("Residential address of the person.", address["description"]);
    }

    [Fact]
    public void ContainerProperty_IsNotDescribed()
    {
        // `model` is the back-reference to MyModel; serializing it would recurse, so it is
        // absent from documents and must be absent from the schema too.
        var properties = Properties(new VmfJsonSchemaGenerator().GenerateSchema<Person>());

        Assert.False(properties.ContainsKey("model"));
    }
}
