// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Linq;
using System.Text.Json;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Internal;

namespace VMF.NET.Json;

/// <summary>
/// Utility methods for VMF JSON serialization decisions.
/// </summary>
public static class VmfTypeUtils
{
    /// <summary>
    /// Determines whether the property should be included in JSON serialization.
    /// Contained properties, external-type properties, and immutable properties are serialized.
    /// Non-contained model-type references (cross-refs) are skipped to avoid circular references.
    /// </summary>
    public static bool ShouldSerialize(VmfProperty prop)
    {
        // Container properties (child side of containment) are never serialized
        if (IsContainerProperty(prop)) return false;

        var type = prop.Type;

        // Non-model-type properties (primitives, strings, externals) always serialize
        if (!type.IsModelType) return true;

        // Check for containment annotation (parent side)
        if (IsContainedProperty(prop)) return true;

        // Immutable model types are values, so they serialize.
        //
        // Decided from the TYPE, as Java does -- isToBeExcludedFromSerialization asks
        // Immutable.class.isAssignableFrom(...). Asking the VALUE instead (prop.Get() is
        // IImmutable) silently drops the property whenever it happens to be null, which on the
        // all-null prototype the schema generator works from is *always*: an immutable-typed
        // property simply never appeared in a generated schema.
        if (IsImmutableType(type)) return true;

        // Non-contained model-type reference — skip (it's a cross-ref)
        return false;
    }

    private static readonly Dictionary<string, bool> _immutableTypeCache = new();

    /// <summary>
    /// Whether a model type (or a list's element type) is immutable.
    /// </summary>
    public static bool IsImmutableType(VmfType type)
    {
        var name = type.IsListType ? type.GetElementTypeName() : type.Name;
        if (name is null) return false;

        lock (_immutableTypeCache)
        {
            if (_immutableTypeCache.TryGetValue(name, out var cached)) return cached;
        }

        var result = false;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var clrType = assembly.GetType(name);
            if (clrType is null) continue;
            result = typeof(IImmutable).IsAssignableFrom(clrType);
            break;
        }

        lock (_immutableTypeCache)
        {
            _immutableTypeCache[name] = result;
        }
        return result;
    }

    /// <summary>
    /// The JSON field name for a property under a naming policy.
    /// <para>
    /// A <c>vmf:json:name</c> rename is used <b>verbatim</b> and never transformed. Java's
    /// <c>getFieldNameForProperty</c> returns the annotation value directly, so a rename means
    /// exactly what it says whatever policy is in force.
    /// </para>
    /// </summary>
    public static string FieldName(VmfProperty prop, JsonNamingPolicy policy)
    {
        var annotation = prop.AnnotationByKey(VmfJsonKeys.Name);
        if (annotation is not null) return annotation.Value;
        return policy.ConvertName(prop.Name);
    }

    /// <summary>
    /// The model types that directly extend <paramref name="type"/>.
    /// <para>
    /// Java's <c>VMFTypeUtils.getSubTypes</c> filters the model's types by
    /// <c>superTypes().contains(type)</c>. A namespace is one model in VMF.NET, so that is the
    /// set searched here.
    /// </para>
    /// </summary>
    public static IReadOnlyList<VmfType> GetSubTypes(VmfType type)
    {
        if (!type.IsModelType || type.IsListType) return System.Array.Empty<VmfType>();

        var lastDot = type.Name.LastIndexOf('.');
        var candidates = lastDot > 0
            ? VmfTypeRegistry.AllInNamespace(type.Name.Substring(0, lastDot))
            : VmfTypeRegistry.All();

        return candidates
            .Where(t => t.SuperTypes().Any(
                s => string.Equals(s.Name, type.Name, System.StringComparison.Ordinal)))
            .ToList();
    }

    /// <summary>
    /// Returns the JSON field name for a property, checking for rename annotations.
    /// Applies no naming policy — prefer <see cref="FieldName(VmfProperty, JsonNamingPolicy)"/>.
    /// </summary>
    public static string GetFieldName(VmfProperty prop)
    {
        var annotation = prop.AnnotationByKey(VmfJsonKeys.Name);
        if (annotation is not null) return annotation.Value;
        return prop.Name;
    }

    /// <summary>
    /// Checks if a VMF object type is polymorphic (has supertypes that are used as property types elsewhere).
    /// </summary>
    public static bool IsPolymorphic(IVObject obj)
    {
        var type = obj.VMF.Reflect.Type();
        var allTypes = obj.VMF.Reflect.AllTypes();

        // Collect all types used as property types
        var propTypes = new HashSet<string>();
        foreach (var t in allTypes)
        {
            if (t.IsInterfaceOnly) continue;

            // Get properties via a prototype if we have one
            // For the current object's type, use its own reflect
            var props = GetPropertiesForType(obj, t);
            foreach (var p in props)
            {
                if (p.Type.IsListType)
                {
                    var elemName = p.Type.GetElementTypeName();
                    if (elemName is not null) propTypes.Add(elemName);
                }
                else if (p.Type.IsModelType)
                {
                    propTypes.Add(p.Type.Name);
                }
            }
        }

        // Check if any supertype of this type is used as a property type
        foreach (var superType in type.SuperTypes())
        {
            if (propTypes.Contains(superType.Name)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the property represents a contained child (parent side of containment).
    /// The annotation value starts with "contained:" for these properties.
    /// </summary>
    public static bool IsContainedProperty(VmfProperty prop)
    {
        var annotation = prop.AnnotationByKey("vmf:property:containment-info");
        if (annotation is not null)
        {
            return annotation.Value.StartsWith("contained");
        }
        return false;
    }

    /// <summary>
    /// Checks if the property is a container reference (child side of containment).
    /// The annotation value starts with "container:" for these properties.
    /// </summary>
    public static bool IsContainerProperty(VmfProperty prop)
    {
        var annotation = prop.AnnotationByKey("vmf:property:containment-info");
        if (annotation is not null)
        {
            return annotation.Value.StartsWith("container");
        }
        return false;
    }

    private static IReadOnlyList<VmfProperty> GetPropertiesForType(IVObject context, VmfType type)
    {
        // The context object's own type needs no prototype.
        if (context.VMF.Reflect.Type().Name == type.Name)
            return context.VMF.Reflect.Properties();

        // Any other type is reached through static reflection. This used to return empty, with
        // the note that properties could not be had without an instance -- which quietly made
        // IsPolymorphic approximate: a supertype used as a property type on some OTHER type went
        // unseen, and the @vmf-type discriminator was then omitted where it was needed.
        try
        {
            return type.Reflect().Properties();
        }
        catch (InvalidOperationException)
        {
            // No prototype factory: interface-only or otherwise not instantiable.
            return Array.Empty<VmfProperty>();
        }
    }
}
