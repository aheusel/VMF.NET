// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Reflection;
using System.Text.Json;
using VMF.NET.Runtime;

namespace VMF.NET.Json;

/// <summary>
/// Generates JSON Schema (draft-07) from VMF model types.
/// Uses the reflection API to discover properties and generate schema definitions.
/// </summary>
public sealed class VmfJsonSchemaGenerator
{
    private readonly Dictionary<string, string> _typeAliases = new();
    private readonly Dictionary<string, string> _typeAliasesReverse = new();

    /// <summary>Adds a type alias mapping for schema generation.</summary>
    public VmfJsonSchemaGenerator WithTypeAlias(string alias, string fullTypeName)
    {
        _typeAliases[alias] = fullTypeName;
        _typeAliasesReverse[fullTypeName] = alias;
        return this;
    }

    /// <summary>Generates a JSON Schema for the specified VMF model type.</summary>
    public Dictionary<string, object> GenerateSchema<T>() where T : IVObject
    {
        return GenerateSchema(typeof(T));
    }

    /// <summary>Generates a JSON Schema for the specified VMF model type.</summary>
    public Dictionary<string, object> GenerateSchema(System.Type modelType)
    {
        var prototype = CreatePrototype(modelType);
        if (prototype is null)
            throw new InvalidOperationException($"Cannot create prototype for type '{modelType.Name}'.");

        var reflect = prototype.VMF.Reflect;
        var schema = new Dictionary<string, object>
        {
            ["$schema"] = "http://json-schema.org/draft-07/schema#",
            ["title"] = reflect.Type().Name,
            ["type"] = "object"
        };

        var properties = new Dictionary<string, object>();
        foreach (var prop in reflect.Properties())
        {
            if (VmfTypeUtils.IsContainerProperty(prop)) continue;
            if (!VmfTypeUtils.ShouldSerialize(prop)) continue;

            properties[VmfTypeUtils.GetFieldName(prop)] = GeneratePropertySchema(prop);
        }
        schema["properties"] = properties;

        // Definitions describe the types this schema actually references, reached transitively
        // from the root. Emitting one for every type in the model would mean an unrelated type --
        // one carrying a malformed annotation, say -- breaks schema generation for every valid
        // type beside it.
        var reachable = ReachableModelTypeNames(reflect);

        var definitions = new Dictionary<string, object>();
        foreach (var type in reflect.AllTypes())
        {
            if (type.IsInterfaceOnly) continue;
            if (!reachable.Contains(type.Name)) continue;
            var typeProto = CreatePrototype(type.Name);
            if (typeProto is null) continue;

            var typeDef = new Dictionary<string, object> { ["type"] = "object" };
            var typeProps = new Dictionary<string, object>();
            foreach (var p in typeProto.VMF.Reflect.Properties())
            {
                if (VmfTypeUtils.IsContainerProperty(p)) continue;
                if (!VmfTypeUtils.ShouldSerialize(p)) continue;
                typeProps[VmfTypeUtils.GetFieldName(p)] = GeneratePropertySchema(p);
            }
            typeDef["properties"] = typeProps;
            definitions[GetTypeAlias(type.Name)] = typeDef;
        }

        if (definitions.Count > 0)
            schema["definitions"] = definitions;

        return schema;
    }

    /// <summary>Generates the JSON Schema as a formatted JSON string.</summary>
    public string GenerateSchemaAsString<T>() where T : IVObject
    {
        return JsonSerializer.Serialize(GenerateSchema<T>(), new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>Generates the JSON Schema as a formatted JSON string.</summary>
    public string GenerateSchemaAsString(System.Type modelType)
    {
        return JsonSerializer.Serialize(GenerateSchema(modelType), new JsonSerializerOptions { WriteIndented = true });
    }

    private Dictionary<string, object> GeneratePropertySchema(VmfProperty prop)
    {
        var schema = new Dictionary<string, object>();
        var type = prop.Type;

        if (!type.IsModelType && !type.IsListType)
        {
            schema["type"] = MapValueType(type.Name);
            AddDefaultIfAvailable(prop, schema);
        }
        else if (type.IsModelType && !type.IsListType)
        {
            schema["$ref"] = $"#/definitions/{GetTypeAlias(type.Name)}";
        }
        else if (type.IsListType)
        {
            schema["type"] = "array";
            var elemName = type.GetElementTypeName();
            if (elemName is not null)
            {
                var itemSchema = new Dictionary<string, object>();
                if (IsValueTypeName(elemName))
                    itemSchema["type"] = MapValueType(elemName);
                else
                    itemSchema["$ref"] = $"#/definitions/{GetTypeAlias(elemName)}";
                schema["items"] = itemSchema;
            }
        }

        // Add annotation-driven schema properties
        AddAnnotationProperties(prop, schema);

        return schema;
    }

    private static void AddAnnotationProperties(VmfProperty prop, Dictionary<string, object> schema)
    {
        AddDefaultIfAvailable(prop, schema);
        AddStringAnnotation(prop, schema, VmfSchemaKeys.Description, "description");
        AddStringAnnotation(prop, schema, VmfSchemaKeys.Format, "format");
        AddStringAnnotation(prop, schema, VmfSchemaKeys.Title, "title");
        AddConstraints(prop, schema);
        AddUniqueItems(prop, schema);
        AddPropertyOrder(prop, schema);
        AddInjections(prop, schema);
    }

    private static void AddDefaultIfAvailable(VmfProperty prop, Dictionary<string, object> schema)
    {
        try
        {
            var defaultValue = prop.GetDefault();
            if (defaultValue is not null)
                schema["default"] = defaultValue;
        }
        catch
        {
            // Ignore if default is not available
        }
    }

    private static void AddStringAnnotation(VmfProperty prop, Dictionary<string, object> schema, string annotationKey, string schemaKey)
    {
        var annotation = prop.AnnotationByKey(annotationKey);
        if (annotation is not null)
            schema[schemaKey] = annotation.Value;
    }

    // JSON Schema (draft-07) keywords with a known value type. The 'constraint' catch-all stays
    // open-ended — unknown keywords are still accepted (see ParseConstraintValue) — but for these
    // we validate the value so a mistake is reported instead of silently producing an invalid
    // schema. (Deliberately no "did you mean 'minimum'?" for unknown keywords: rejecting unknowns
    // would forfeit the catch-all's open-ended extension point.)
    private static readonly HashSet<string> NumericConstraints = new(StringComparer.Ordinal)
        { "minimum", "maximum", "exclusiveMinimum", "exclusiveMaximum", "multipleOf" };
    private static readonly HashSet<string> IntegerConstraints = new(StringComparer.Ordinal)
        { "minLength", "maxLength", "minItems", "maxItems", "minProperties", "maxProperties" };
    private static readonly HashSet<string> BooleanConstraints = new(StringComparer.Ordinal)
        { "uniqueItems", "readOnly", "writeOnly" };
    private static readonly HashSet<string> JsonValuedConstraints = new(StringComparer.Ordinal)
        { "default", "const", "enum", "examples" };

    private static void AddConstraints(VmfProperty prop, Dictionary<string, object> schema)
    {
        // Supports multiple constraint annotations, each "keyword=value"
        // e.g., [VmfAnnotation(Key = "vmf:schema:constraint", Value = "pattern=^\\d{3}$")]
        // e.g., [VmfAnnotation(Key = "vmf:schema:constraint", Value = "minimum=0")]
        foreach (var annotation in prop.Annotations())
        {
            if (annotation.Key != VmfSchemaKeys.Constraint) continue;
            var value = annotation.Value;

            // Split at the FIRST '=' so values may contain '=' (regex lookaheads, defaults, ...).
            var eqIndex = value?.IndexOf('=') ?? -1;
            if (eqIndex <= 0)
                throw Malformed(prop, VmfSchemaKeys.Constraint, value, "expected 'keyword=value'");

            var keyword = value!.Substring(0, eqIndex).Trim();
            var raw = value.Substring(eqIndex + 1).Trim();
            if (keyword.Length == 0 || raw.Length == 0)
                throw Malformed(prop, VmfSchemaKeys.Constraint, value, "expected 'keyword=value' with non-empty sides");

            schema[keyword] = ParseConstraintValue(prop, keyword, raw);
        }
    }

    private static object ParseConstraintValue(VmfProperty prop, string keyword, string raw)
    {
        if (NumericConstraints.Contains(keyword))
        {
            if (int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var intVal)) return intVal;
            if (double.TryParse(raw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var dblVal)) return dblVal;
            throw ConstraintTypeError(prop, keyword, raw, "a numeric value");
        }

        if (IntegerConstraints.Contains(keyword))
        {
            if (int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var intVal)) return intVal;
            throw ConstraintTypeError(prop, keyword, raw, "an integer value");
        }

        if (BooleanConstraints.Contains(keyword))
        {
            if (bool.TryParse(raw, out var boolVal)) return boolVal;
            throw ConstraintTypeError(prop, keyword, raw, "a boolean value ('true' or 'false')");
        }

        if (keyword == "pattern")
        {
            try { _ = new System.Text.RegularExpressions.Regex(raw); }
            catch (ArgumentException ex) { throw ConstraintTypeError(prop, keyword, raw, "a valid regular expression", ex.Message); }
            return raw;
        }

        // JSON-valued keywords, and any array/object value, are parsed as real JSON so that e.g.
        // default=["a","b"] renders a JSON array — not the string "[\"a\",\"b\"]".
        if (JsonValuedConstraints.Contains(keyword) || raw.StartsWith("[") || raw.StartsWith("{"))
        {
            try { return JsonSerializer.Deserialize<JsonElement>(raw); }
            catch (JsonException ex) { throw ConstraintTypeError(prop, keyword, raw, "valid JSON", ex.Message); }
        }

        // Unknown scalar keyword: keep the open-ended catch-all with smart scalar coercion.
        if (int.TryParse(raw, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var i)) return i;
        if (double.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
        if (bool.TryParse(raw, out var b)) return b;
        return raw;
    }

    private static VmfSchemaAnnotationException Malformed(VmfProperty prop, string key, string? value, string expectation)
        => new($"Property '{prop.Name}': annotation '{key}' is malformed ({expectation}) but got '{value}'.");

    private static VmfSchemaAnnotationException ConstraintTypeError(
        VmfProperty prop, string keyword, string raw, string expected, string? detail = null)
        => new($"Property '{prop.Name}': schema constraint '{keyword}' expects {expected} but got '{raw}'."
               + (detail is null ? "" : $" ({detail})"));

    private static void AddUniqueItems(VmfProperty prop, Dictionary<string, object> schema)
    {
        var annotation = prop.AnnotationByKey(VmfSchemaKeys.UniqueItems);
        if (annotation is null) return;
        if (!bool.TryParse(annotation.Value, out var unique))
            throw Malformed(prop, VmfSchemaKeys.UniqueItems, annotation.Value, "expected a boolean ('true' or 'false')");
        schema["uniqueItems"] = unique;
    }

    private static void AddPropertyOrder(VmfProperty prop, Dictionary<string, object> schema)
    {
        var annotation = prop.AnnotationByKey(VmfSchemaKeys.PropertyOrder);
        if (annotation is null) return;
        if (!int.TryParse(annotation.Value, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var order))
            throw Malformed(prop, VmfSchemaKeys.PropertyOrder, annotation.Value, "expected an integer");
        schema["propertyOrder"] = order;
    }

    private static void AddInjections(VmfProperty prop, Dictionary<string, object> schema)
    {
        // Injects arbitrary JSON key-value pairs into the schema.
        // Value is raw JSON fragment without outer braces, e.g., "\"examples\":[1,2,3]"
        var annotation = prop.AnnotationByKey(VmfSchemaKeys.Inject);
        if (annotation is null) return;

        Dictionary<string, JsonElement>? injected;
        try
        {
            injected = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("{" + annotation.Value + "}");
        }
        catch (JsonException ex)
        {
            throw new VmfSchemaAnnotationException(
                $"Property '{prop.Name}': annotation '{VmfSchemaKeys.Inject}' is not valid JSON: '{annotation.Value}'. ({ex.Message})", ex);
        }

        if (injected is null) return;
        foreach (var (key, val) in injected)
            schema[key] = val;
    }

    private static string MapValueType(string typeName) => typeName switch
    {
        "System.Int32" or "System.Int16" or "System.Int64" or "System.Byte"
            or "int" or "short" or "long" or "byte" => "integer",
        "System.Boolean" or "bool" => "boolean",
        "System.Double" or "System.Single" or "System.Decimal"
            or "double" or "float" or "decimal" => "number",
        _ => "string"
    };

    private static bool IsValueTypeName(string typeName) => typeName switch
    {
        "System.Int32" or "System.Int16" or "System.Int64" or "System.Byte"
            or "System.Boolean" or "System.Double" or "System.Single" or "System.Decimal"
            or "System.String" or "System.Char"
            or "int" or "short" or "long" or "byte" or "bool"
            or "double" or "float" or "decimal" or "string" or "char" => true,
        _ => false
    };

    private string GetTypeAlias(string typeName)
    {
        return _typeAliasesReverse.TryGetValue(typeName, out var alias) ? alias : typeName;
    }

    private static IVObject? CreatePrototype(System.Type type)
    {
        var builderMethod = type.GetMethod("NewBuilder", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (builderMethod is null)
        {
            foreach (var iface in type.GetInterfaces())
            {
                builderMethod = iface.GetMethod("NewBuilder", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (builderMethod is not null) break;
            }
        }
        if (builderMethod is null) return null;

        try
        {
            var builder = (IBuilder)builderMethod.Invoke(null, null)!;
            return builder.Build();
        }
        catch
        {
            // If Build() fails (e.g., required properties), try NewInstance() instead
            var newInstanceMethod = type.GetMethod("NewInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (newInstanceMethod is null)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    newInstanceMethod = iface.GetMethod("NewInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (newInstanceMethod is not null) break;
                }
            }

            try
            {
                return newInstanceMethod?.Invoke(null, null) as IVObject;
            }
            catch
            {
                return null;
            }
        }
    }


    /// <summary>
    /// The names of the model types this schema refers to: everything reachable from the root
    /// type through model-typed properties and model-typed list elements, transitively.
    /// </summary>
    private HashSet<string> ReachableModelTypeNames(VMF.NET.Runtime.IReflect root)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<VMF.NET.Runtime.IReflect>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            foreach (var prop in queue.Dequeue().Properties())
            {
                if (VmfTypeUtils.IsContainerProperty(prop)) continue;
                if (!VmfTypeUtils.ShouldSerialize(prop)) continue;

                string? name = prop.Type.IsListType ? prop.Type.GetElementTypeName()
                             : prop.Type.IsModelType ? prop.Type.Name
                             : null;

                if (name is null || !seen.Add(name)) continue;

                var proto = CreatePrototype(name);
                if (proto is not null) queue.Enqueue(proto.VMF.Reflect);
            }
        }

        return seen;
    }
    private IVObject? CreatePrototype(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType(typeName);
            if (type is not null) return CreatePrototype(type);
        }
        return null;
    }
}
