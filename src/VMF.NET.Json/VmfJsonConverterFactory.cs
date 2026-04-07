// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Text.Json;
using System.Text.Json.Serialization;
using VMF.NET.Runtime;

namespace VMF.NET.Json;

/// <summary>
/// System.Text.Json converter factory for VMF model objects.
/// Supports polymorphic serialization/deserialization via <c>@vmf-type</c> discriminator.
/// <para>
/// Usage:
/// <code>
/// var options = new JsonSerializerOptions();
/// options.Converters.Add(new VmfJsonConverterFactory());
/// var json = JsonSerializer.Serialize(myVmfObject, options);
/// </code>
/// </para>
/// </summary>
public sealed class VmfJsonConverterFactory : JsonConverterFactory
{
    private readonly Dictionary<string, string> _typeAliases = new();
    private readonly Dictionary<string, string> _typeAliasesReverse = new();

    /// <summary>
    /// Adds a type alias for polymorphic serialization.
    /// The alias is used as the <c>@vmf-type</c> discriminator value instead of the full type name.
    /// </summary>
    public VmfJsonConverterFactory WithTypeAlias(string alias, string fullTypeName)
    {
        _typeAliases[alias] = fullTypeName;
        _typeAliasesReverse[fullTypeName] = alias;
        return this;
    }

    /// <summary>
    /// Adds multiple type aliases at once.
    /// </summary>
    public VmfJsonConverterFactory WithTypeAliases(IDictionary<string, string> aliases)
    {
        foreach (var kvp in aliases)
        {
            WithTypeAlias(kvp.Key, kvp.Value);
        }
        return this;
    }

    internal IReadOnlyDictionary<string, string> TypeAliases => _typeAliases;
    internal IReadOnlyDictionary<string, string> TypeAliasesReverse => _typeAliasesReverse;

    public override bool CanConvert(System.Type typeToConvert)
    {
        return typeof(IVObject).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(System.Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(VmfJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, this)!;
    }
}
