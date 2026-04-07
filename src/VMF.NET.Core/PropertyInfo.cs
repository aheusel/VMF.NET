// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace VMF.NET.Core;

/// <summary>
/// Represents a single property in a VMF model type.
/// Port of Java Prop.java.
/// </summary>
public sealed class PropertyInfo : IEquatable<PropertyInfo>
{
    public PropertyInfo(ModelTypeInfo parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    /// <summary>The model type that owns this property.</summary>
    public ModelTypeInfo Parent { get; }

    /// <summary>Property name (PascalCase in C#).</summary>
    public string Name { get; }

    /// <summary>Full type name including namespace.</summary>
    public string TypeName { get; set; } = "";

    /// <summary>Simple type name (without namespace).</summary>
    public string SimpleTypeName { get; set; } = "";

    /// <summary>The namespace of the property type.</summary>
    public string PackageName { get; set; } = "";

    /// <summary>Property type classification.</summary>
    public PropType PropType { get; set; } = PropType.Class;

    /// <summary>Resolved model type (null for external/primitive types).</summary>
    public ModelTypeInfo? ModelType { get; set; }

    // --- Collection type info ---

    /// <summary>For collection properties: the element type name.</summary>
    public string? GenericTypeName { get; set; }

    /// <summary>For collection properties: the element type namespace.</summary>
    public string? GenericPackageName { get; set; }

    /// <summary>For collection properties: the resolved element model type.</summary>
    public ModelTypeInfo? GenericModelType { get; set; }

    // --- Relationship info ---

    /// <summary>Containment information.</summary>
    public ContainmentInfo Containment { get; set; } = ContainmentInfo.None;

    /// <summary>Cross-reference information (null if not a cross-ref).</summary>
    public ReferenceInfo? Reference { get; set; }

    // --- Flags ---

    /// <summary>Whether this property is required.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Whether this property is excluded from equals/hashCode.</summary>
    public bool IsIgnoredForEquals { get; set; }

    /// <summary>Whether this property is excluded from ToString.</summary>
    public bool IsIgnoredForToString { get; set; }

    /// <summary>Whether only a getter is generated (no setter).</summary>
    public bool IsGetterOnly { get; set; }

    /// <summary>Whether this property is read-only (container properties).</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>Custom default value expression as string.</summary>
    public string? DefaultValueAsString { get; set; }

    /// <summary>Custom property order index.</summary>
    public int? CustomOrderIndex { get; set; }

    /// <summary>Property ID (sequential index within the implementation).</summary>
    public int PropId { get; set; }

    /// <summary>Custom documentation text.</summary>
    public string? Documentation { get; set; }

    /// <summary>Annotations on this property.</summary>
    public List<AnnotationInfo> Annotations { get; } = new();

    // --- Computed properties ---

    /// <summary>Whether this property's type is a model type.</summary>
    public bool IsModelType => ModelType != null;

    /// <summary>Whether this is a collection property.</summary>
    public bool IsCollectionType => PropType == PropType.Collection;

    /// <summary>Whether this is a containment property (either container or contained).</summary>
    public bool IsContainmentProperty => Containment.ContainmentType != ContainmentType.None;

    /// <summary>Whether this property is the contained side.</summary>
    public bool IsContained => Containment.ContainmentType == ContainmentType.Contained;

    /// <summary>Whether this property is the container side.</summary>
    public bool IsContainer => Containment.ContainmentType == ContainmentType.Container;

    /// <summary>Whether this is a cross-reference property.</summary>
    public bool IsCrossRefProperty => Reference != null;

    /// <summary>Whether this property has documentation.</summary>
    public bool IsDocumented => !string.IsNullOrEmpty(Documentation);

    /// <summary>
    /// The type ID for this property's type.
    /// Model type: the type's typeId. Collection: -2. External: -1.
    /// </summary>
    public int GetTypeId()
    {
        if (ModelType != null) return ModelType.TypeId;
        if (IsCollectionType) return -2;
        return -1;
    }

    /// <summary>Default value string for code generation ("null" if none specified).</summary>
    public string GetDefaultValueForCodeGen()
    {
        if (string.IsNullOrEmpty(DefaultValueAsString)) return "default";
        return DefaultValueAsString;
    }

    public bool Equals(PropertyInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && PackageName == other.PackageName && TypeName == other.TypeName;
    }

    public override bool Equals(object? obj) => Equals(obj as PropertyInfo);
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Name.GetHashCode();
            hash = hash * 31 + PackageName.GetHashCode();
            hash = hash * 31 + TypeName.GetHashCode();
            return hash;
        }
    }
    public override string ToString() => $"{TypeName} {Name}";
}
