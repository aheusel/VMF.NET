// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using VMF.NET.Runtime;

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

        // Immutable scalar properties
        var value = prop.Get();
        if (value is IImmutable) return true;

        // Immutable collection elements: check if list element type implements IImmutable
        if (type.IsListType && value is System.Collections.IList list)
        {
            // If the list has items, check the first one
            if (list.Count > 0 && list[0] is IImmutable) return true;
            // Even if empty, check the element type via reflection
            var listType = value.GetType();
            if (listType.IsGenericType)
            {
                var elementType = listType.GetGenericArguments()[0];
                if (typeof(IImmutable).IsAssignableFrom(elementType)) return true;
            }
        }

        // Non-contained model-type reference — skip (it's a cross-ref)
        return false;
    }

    /// <summary>
    /// Returns the JSON field name for a property, checking for rename annotations.
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
        var type = obj.Vmf().Reflect().Type();
        var allTypes = obj.Vmf().Reflect().AllTypes();

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
        if (context.Vmf().Reflect().Type().Name == type.Name)
            return context.Vmf().Reflect().Properties();

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
