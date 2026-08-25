// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VMF.NET.Core;

/// <summary>
/// Root container for a VMF model. Holds all types, resolves opposites, and validates
/// the model after multi-pass analysis.
/// Port of Java Model.java.
/// </summary>
public sealed class ModelInfo
{
    private readonly Dictionary<string, ModelTypeInfo> _types = new();

    public ModelInfo(string namespaceName)
    {
        NamespaceName = namespaceName;
    }

    /// <summary>The target namespace for generated code.</summary>
    public string NamespaceName { get; }

    /// <summary>Model-wide configuration.</summary>
    public ModelConfig Config { get; set; } = ModelConfig.Default;

    /// <summary>All model types, sorted by full type name.</summary>
    public IReadOnlyList<ModelTypeInfo> Types =>
        _types.Values.OrderBy(t => t.FullTypeName).ToList();

    /// <summary>Diagnostics/errors encountered during analysis.</summary>
    public List<Diagnostic> Diagnostics { get; } = new();

    /// <summary>Whether the model has any errors.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>Registers a type in the model.</summary>
    public ModelTypeInfo AddType(string typeName, int typeId)
    {
        var type = new ModelTypeInfo(this, typeName, NamespaceName, typeId);
        _types[NamespaceName + "." + typeName] = type;
        return type;
    }

    /// <summary>Resolves a type by full name.</summary>
    public ModelTypeInfo? ResolveType(string fullName)
    {
        _types.TryGetValue(fullName, out var result);
        return result;
    }

    /// <summary>Whether the given full name is a known model type.</summary>
    public bool IsModelType(string fullName) => _types.ContainsKey(fullName);

    /// <summary>
    /// Resolves the opposite property from a "TypeName.PropName" reference.
    /// </summary>
    public PropertyInfo? ResolveOpposite(ModelTypeInfo ownerType, string oppositeRef)
    {
        var parts = oppositeRef.Split('.');

        if (parts.Length <= 1) return null;

        // If only "TypeName.PropName" (no namespace), assume model namespace
        string fullRef = parts.Length == 2
            ? $"{NamespaceName}.{oppositeRef}"
            : oppositeRef;

        var typeParts = fullRef.Split('.');
        var propName = typeParts[typeParts.Length - 1];
        var typeNameParts = new string[typeParts.Length - 1];
        Array.Copy(typeParts, typeNameParts, typeParts.Length - 1);
        var typeName = string.Join(".", typeNameParts);

        var targetType = ResolveType(typeName);
        return targetType?.ResolveProp(propName);
    }

    /// <summary>
    /// Returns all properties in the model that contain instances of the specified type
    /// (via [Contains]), optionally filtering by presence of an opposite.
    /// </summary>
    public List<PropertyInfo> FindAllPropsThatContainType(ModelTypeInfo type, bool withOpposite)
    {
        var result = new List<PropertyInfo>();
        foreach (var t in Types)
        {
            foreach (var p in t.Properties)
            {
                var pType = p.ModelType ?? (p.IsCollectionType ? p.GenericModelType : null);
                if (pType == null) continue;

                bool matchesOpposite = withOpposite
                    ? !p.Containment.IsWithoutOpposite
                    : p.Containment.IsWithoutOpposite;

                // `type` must be ASSIGNABLE TO the property's element/target type: only then
                // can an instance of `type` actually sit in that property. Matching the other
                // direction as well made a base type emit cleanup casting `this` to a derived
                // type, which does not compile.
                if (p.IsContainmentProperty
                    && matchesOpposite
                    && p.Containment.ContainmentType == ContainmentType.Contained
                    && type.ExtendsType(pType))
                {
                    result.Add(p);
                }
            }
        }
        return result;
    }

    /// <summary>Whether the type is contained by any other type.</summary>
    public bool IsContained(ModelTypeInfo type)
    {
        return FindAllPropsThatContainType(type, false).Count > 0
            || FindAllPropsThatContainType(type, true).Count > 0;
    }

    /// <summary>All types that implement (extend) the specified type.</summary>
    public List<ModelTypeInfo> GetAllTypesThatImplement(ModelTypeInfo type)
    {
        return Types.Where(t => t.Implements.Contains(type)).Distinct().ToList();
    }

    /// <summary>Adds an error diagnostic.</summary>
    public void AddError(string message, string? location = null, string id = Diagnostic.DefaultId)
    {
        Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, location, id));
    }

    /// <summary>Adds a warning diagnostic.</summary>
    public void AddWarning(string message, string? location = null, string id = Diagnostic.DefaultId)
    {
        Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, message, location, id));
    }
}

/// <summary>Severity of a model diagnostic.</summary>
public enum DiagnosticSeverity
{
    Warning,
    Error
}

/// <summary>A diagnostic message from model analysis.</summary>
public sealed class Diagnostic
{
    /// <summary>Diagnostic id used when none is given: general model analysis.</summary>
    public const string DefaultId = "VMF001";

    /// <summary>
    /// A leading <c>I</c> was stripped to name the implementation class, so
    /// <c>IHorse</c> is implemented by <c>HorseImpl</c>.
    /// </summary>
    public const string PrefixStrippedId = "VMF004";

    public Diagnostic(DiagnosticSeverity severity, string message, string? location = null,
        string id = DefaultId)
    {
        Severity = severity;
        Message = message;
        Location = location;
        Id = id;
    }

    public DiagnosticSeverity Severity { get; }
    public string Message { get; }
    public string? Location { get; }

    /// <summary>
    /// The reported diagnostic id. Distinct ids exist so a consumer can silence one kind without
    /// silencing model analysis as a whole — <c>&lt;NoWarn&gt;VMF004&lt;/NoWarn&gt;</c>.
    /// </summary>
    public string Id { get; }

    public override string ToString() => $"[{Severity}] {Message}" + (Location != null ? $" at {Location}" : "");
}
