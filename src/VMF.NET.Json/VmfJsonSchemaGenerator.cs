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

        var reflect = prototype.Vmf().Reflect();
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

        // Generate definitions for all model types
        var definitions = new Dictionary<string, object>();
        foreach (var type in reflect.AllTypes())
        {
            if (type.IsInterfaceOnly) continue;
            var typeProto = CreatePrototype(type.Name);
            if (typeProto is null) continue;

            var typeDef = new Dictionary<string, object> { ["type"] = "object" };
            var typeProps = new Dictionary<string, object>();
            foreach (var p in typeProto.Vmf().Reflect().Properties())
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
        AddStringAnnotation(prop, schema, "vmf:jackson:schema:description", "description");
        AddStringAnnotation(prop, schema, "vmf:jackson:schema:format", "format");
        AddStringAnnotation(prop, schema, "vmf:jackson:schema:title", "title");
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

    private static void AddConstraints(VmfProperty prop, Dictionary<string, object> schema)
    {
        // Supports multiple constraint annotations, each with format "key=value"
        // e.g., [VmfAnnotation(Key = "vmf:jackson:schema:constraint", Value = "pattern=^\\d{3}$")]
        // e.g., [VmfAnnotation(Key = "vmf:jackson:schema:constraint", Value = "minimum=0")]
        foreach (var annotation in prop.Annotations())
        {
            if (annotation.Key != "vmf:jackson:schema:constraint") continue;
            var value = annotation.Value;
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('=')) continue;

            // Split at first '=' so values can contain '='
            var eqIndex = value.IndexOf('=');
            var constraintName = value.Substring(0, eqIndex).Trim();
            var constraintValue = value.Substring(eqIndex + 1).Trim();

            if (string.IsNullOrEmpty(constraintName) || string.IsNullOrEmpty(constraintValue)) continue;

            // Try to parse as number or boolean; fall back to string
            schema[constraintName] = ParseConstraintValue(constraintValue);
        }
    }

    private static object ParseConstraintValue(string value)
    {
        if (int.TryParse(value, out var intVal)) return intVal;
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var dblVal)) return dblVal;
        if (bool.TryParse(value, out var boolVal)) return boolVal;
        return value;
    }

    private static void AddUniqueItems(VmfProperty prop, Dictionary<string, object> schema)
    {
        var annotation = prop.AnnotationByKey("vmf:jackson:schema:uniqueItems");
        if (annotation is not null && bool.TryParse(annotation.Value, out var unique))
            schema["uniqueItems"] = unique;
    }

    private static void AddPropertyOrder(VmfProperty prop, Dictionary<string, object> schema)
    {
        var annotation = prop.AnnotationByKey("vmf:jackson:schema:propertyOrder");
        if (annotation is not null && int.TryParse(annotation.Value, out var order))
            schema["propertyOrder"] = order;
    }

    private static void AddInjections(VmfProperty prop, Dictionary<string, object> schema)
    {
        // Injects arbitrary JSON key-value pairs into the schema.
        // Value is raw JSON fragment without outer braces, e.g., "\"examples\":[1,2,3]"
        var annotation = prop.AnnotationByKey("vmf:jackson:schema:inject");
        if (annotation is null) return;

        try
        {
            var json = "{" + annotation.Value + "}";
            var injected = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (injected is not null)
            {
                foreach (var (key, val) in injected)
                {
                    schema[key] = val;
                }
            }
        }
        catch
        {
            // Ignore malformed injection JSON
        }
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
