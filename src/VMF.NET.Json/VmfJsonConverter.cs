// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using VMF.NET.Runtime;

namespace VMF.NET.Json;

internal sealed class VmfJsonConverter<T> : JsonConverter<T> where T : IVObject
{
    private readonly VmfJsonConverterFactory _factory;

    public VmfJsonConverter(VmfJsonConverterFactory factory)
    {
        _factory = factory;
    }

    public override T? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var actualType = ResolveActualType(root, typeToConvert);
        var obj = DeserializeObject(root, actualType, options);
        return (T?)obj;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        SerializeObject(writer, value, options);
    }

    private void SerializeObject(Utf8JsonWriter writer, IVObject obj, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var reflect = obj.Vmf().Reflect();
        var type = reflect.Type();

        // Write @vmf-type discriminator if the type is polymorphic
        if (VmfTypeUtils.IsPolymorphic(obj))
        {
            string typeName = type.Name;
            if (_factory.TypeAliasesReverse.TryGetValue(typeName, out var alias))
                typeName = alias;

            writer.WriteString("@vmf-type", typeName);
        }

        // Write each serializable property
        foreach (var prop in reflect.Properties())
        {
            if (!VmfTypeUtils.ShouldSerialize(prop)) continue;

            var propValue = prop.Get();
            if (propValue is null) continue;

            string fieldName = VmfTypeUtils.GetFieldName(prop);
            writer.WritePropertyName(ApplyNamingPolicy(fieldName, options));

            WriteValue(writer, propValue, prop.Type, options);
        }

        writer.WriteEndObject();
    }

    private void WriteValue(Utf8JsonWriter writer, object value, VmfType type, JsonSerializerOptions options)
    {
        if (value is IVObject vmfObj)
        {
            SerializeObject(writer, vmfObj, options);
        }
        else if (type.IsListType && value is IList list)
        {
            writer.WriteStartArray();
            foreach (var item in list)
            {
                if (item is null)
                {
                    writer.WriteNullValue();
                }
                else if (item is IVObject childObj)
                {
                    SerializeObject(writer, childObj, options);
                }
                else
                {
                    JsonSerializer.Serialize(writer, item, item.GetType(), options);
                }
            }
            writer.WriteEndArray();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    private IVObject? DeserializeObject(JsonElement element, System.Type targetType, JsonSerializerOptions options)
    {
        // Find the builder via reflection: IMyType.NewBuilder()
        var builderMethod = FindNewBuilderMethod(targetType);
        if (builderMethod is null)
            throw new JsonException($"Cannot deserialize type '{targetType.Name}': no NewBuilder() method found.");

        var builder = (IBuilder)builderMethod.Invoke(null, null)!;
        var builderType = builder.GetType();

        // Index With* methods by property name (case-insensitive for matching)
        var withMethods = IndexWithMethods(builderType);

        // Build rename map from annotations (renamed JSON field -> property name)
        var renameMap = BuildRenameMap(targetType);

        foreach (var jsonProp in element.EnumerateObject())
        {
            if (jsonProp.Name == "@vmf-type") continue;

            // Find matching With* method by JSON field name
            var withMethod = FindWithMethodByJsonName(withMethods, jsonProp.Name, options, renameMap);
            if (withMethod is null) continue;

            var paramType = withMethod.GetParameters()[0].ParameterType;
            var value = DeserializePropertyValue(jsonProp.Value, paramType, options);

            withMethod.Invoke(builder, new[] { value });
        }

        return builder.Build();
    }

    private object? DeserializePropertyValue(JsonElement element, System.Type paramType, JsonSerializerOptions options)
    {
        if (element.ValueKind == JsonValueKind.Null) return null;

        // Collection property
        if (typeof(IEnumerable).IsAssignableFrom(paramType) && paramType != typeof(string))
        {
            var elementType = paramType.IsArray
                ? paramType.GetElementType()!
                : paramType.GetGenericArguments().FirstOrDefault() ?? typeof(object);

            var items = new List<object?>();
            foreach (var arrayElement in element.EnumerateArray())
            {
                if (typeof(IVObject).IsAssignableFrom(elementType))
                {
                    var actualType = ResolveActualType(arrayElement, elementType);
                    items.Add(DeserializeObject(arrayElement, actualType, options));
                }
                else
                {
                    items.Add(DeserializeScalar(arrayElement, elementType));
                }
            }

            // Builder With* methods take params arrays
            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
                array.SetValue(items[i], i);
            return array;
        }

        // Model-type property
        if (typeof(IVObject).IsAssignableFrom(paramType))
        {
            var actualType = ResolveActualType(element, paramType);
            return DeserializeObject(element, actualType, options);
        }

        // Scalar
        return DeserializeScalar(element, paramType);
    }

    private static object? DeserializeScalar(JsonElement element, System.Type targetType)
    {
        if (element.ValueKind == JsonValueKind.Null) return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying == typeof(string)) return element.GetString();
        if (underlying == typeof(int)) return element.GetInt32();
        if (underlying == typeof(long)) return element.GetInt64();
        if (underlying == typeof(double)) return element.GetDouble();
        if (underlying == typeof(float)) return element.GetSingle();
        if (underlying == typeof(bool)) return element.GetBoolean();
        if (underlying == typeof(short)) return element.GetInt16();
        if (underlying == typeof(byte)) return element.GetByte();
        if (underlying == typeof(char)) return element.GetString()?[0] ?? '\0';
        if (underlying == typeof(decimal)) return element.GetDecimal();
        if (underlying.IsEnum) return Enum.Parse(underlying, element.GetString()!);

        // Fallback: use JsonSerializer
        return JsonSerializer.Deserialize(element.GetRawText(), targetType);
    }

    private System.Type ResolveActualType(JsonElement element, System.Type declaredType)
    {
        if (element.ValueKind != JsonValueKind.Object) return declaredType;

        if (element.TryGetProperty("@vmf-type", out var typeElement))
        {
            var typeName = typeElement.GetString()!;

            // Check aliases first
            if (_factory.TypeAliases.TryGetValue(typeName, out var resolved))
                typeName = resolved;

            var actualType = System.Type.GetType(typeName);
            if (actualType is not null) return actualType;

            // Try searching loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                actualType = asm.GetType(typeName);
                if (actualType is not null) return actualType;
            }
        }

        return declaredType;
    }

    private static MethodInfo? FindNewBuilderMethod(System.Type type)
    {
        // Generated pattern: IMyType.NewBuilder() returns IMyType.Builder
        // Also check the interface hierarchy for the method
        var method = type.GetMethod("NewBuilder", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (method is not null) return method;

        // Check all interfaces
        foreach (var iface in type.GetInterfaces())
        {
            method = iface.GetMethod("NewBuilder", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (method is not null) return method;
        }

        return null;
    }

    /// <summary>
    /// Indexes the builder's With* methods by the property name (extracted from method name).
    /// Only includes single-parameter overloads.
    /// </summary>
    private static Dictionary<string, MethodInfo> IndexWithMethods(System.Type builderType)
    {
        var result = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in builderType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (m.Name.StartsWith("With") && m.Name.Length > 4 && m.GetParameters().Length == 1)
            {
                // Skip if parameter is a builder type
                var paramType = m.GetParameters()[0].ParameterType;
                if (typeof(IBuilder).IsAssignableFrom(paramType)) continue;

                var propName = m.Name.Substring(4); // "WithName" -> "Name"
                result[propName] = m;
            }
        }
        return result;
    }

    /// <summary>
    /// Finds the With* method matching a JSON field name, considering naming policies and rename annotations.
    /// </summary>
    private static MethodInfo? FindWithMethodByJsonName(
        Dictionary<string, MethodInfo> withMethods, string jsonFieldName, JsonSerializerOptions options,
        Dictionary<string, string>? renameMap = null)
    {
        // Check rename map: renamed JSON field -> property name
        if (renameMap is not null)
        {
            foreach (var (propName, renamedField) in renameMap)
            {
                var effectiveName = options.PropertyNamingPolicy?.ConvertName(renamedField) ?? renamedField;
                if (effectiveName == jsonFieldName && withMethods.TryGetValue(propName, out var renamedMethod))
                    return renamedMethod;
            }
        }

        // Direct match (case-insensitive via dictionary)
        if (withMethods.TryGetValue(jsonFieldName, out var method))
            return method;

        // Try to reverse the naming policy: for each With* property name,
        // check if the policy would transform it to the JSON field name
        if (options.PropertyNamingPolicy is not null)
        {
            foreach (var (propName, m) in withMethods)
            {
                if (options.PropertyNamingPolicy.ConvertName(propName) == jsonFieldName)
                    return m;
            }
        }

        return null;
    }

    /// <summary>
    /// Builds a map of property name -> renamed field name from vmf:jackson:rename annotations.
    /// Returns null if no prototype can be created or no renames exist.
    /// </summary>
    private static Dictionary<string, string>? BuildRenameMap(System.Type targetType)
    {
        var prototype = TryCreatePrototype(targetType);
        if (prototype is null) return null;

        Dictionary<string, string>? map = null;
        foreach (var prop in prototype.Vmf().Reflect().Properties())
        {
            var annotation = prop.AnnotationByKey("vmf:jackson:rename");
            if (annotation is not null)
            {
                map ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                map[prop.Name] = annotation.Value;
            }
        }
        return map;
    }

    /// <summary>
    /// Tries to create a prototype instance for reading reflection metadata.
    /// Returns null if the type can't be instantiated (e.g., required properties).
    /// </summary>
    private static IVObject? TryCreatePrototype(System.Type type)
    {
        try
        {
            var builderMethod = FindNewBuilderMethod(type);
            if (builderMethod is null) return null;
            var builder = (IBuilder)builderMethod.Invoke(null, null)!;
            return builder.Build();
        }
        catch
        {
            // Try NewInstance as fallback
            try
            {
                var newInstanceMethod = type.GetMethod("NewInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (newInstanceMethod is null)
                {
                    foreach (var iface in type.GetInterfaces())
                    {
                        newInstanceMethod = iface.GetMethod("NewInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                        if (newInstanceMethod is not null) break;
                    }
                }
                return newInstanceMethod?.Invoke(null, null) as IVObject;
            }
            catch
            {
                return null;
            }
        }
    }

    private static string ApplyNamingPolicy(string name, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
    }
}
