// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Internal interface for VMF object implementations. Provides low-level access
/// to property metadata and values used by the runtime infrastructure.
/// Generated code implements this interface.
/// </summary>
public interface IVObjectInternal : IVObject
{
    /// <summary>Returns the type ID of this object.</summary>
    int GetTypeId();

    /// <summary>Returns the VMF type information.</summary>
    VmfType GetVmfType();

    /// <summary>Returns the names of super types.</summary>
    string[] GetSuperTypeNames();

    /// <summary>Returns the property names.</summary>
    string[] GetPropertyNames();

    /// <summary>
    /// Returns property type codes. -1 = model type, -2 = list type, 0+ = external.
    /// </summary>
    int[] GetPropertyTypes();

    /// <summary>Returns the full type names for each property.</summary>
    string[] GetPropertyTypeNames();

    /// <summary>Returns the property ID for the given name.</summary>
    int GetPropertyIdByName(string name);

    /// <summary>Returns the value of the property at the given index.</summary>
    object? GetPropertyValueById(int id);

    /// <summary>Returns the default value of the property at the given index.</summary>
    object? GetDefaultValueById(int id);

    /// <summary>Returns whether the property at the given index differs from its default.</summary>
    bool IsSetById(int id);

    /// <summary>Returns indices of properties that have model-type or model-element types.</summary>
    int[] GetIndicesOfPropertiesWithModelTypeOrElementTypes();

    /// <summary>Returns indices of properties with model element types (for list properties).</summary>
    int[] GetIndicesOfPropertiesWithModelElementTypes();

    /// <summary>Returns the containment children indices.</summary>
    int[] GetChildrenIndices();

    /// <summary>Returns the containment parent indices.</summary>
    int[] GetParentIndices();

    /// <summary>Returns annotations for the property at the given index.</summary>
    IReadOnlyList<IAnnotation> GetPropertyAnnotationsById(int id);

    /// <summary>Returns type-level annotations for this object.</summary>
    IReadOnlyList<IAnnotation> GetAnnotations();

    /// <summary>Returns whether this is a read-only wrapper.</summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Returns the underlying mutable object (for identity checks on read-only wrappers).
    /// </summary>
    object GetMutableObject();

    /// <summary>
    /// VMF content-based equals with recursion detection.
    /// </summary>
    bool VmfEquals(object? other, HashSet<long> visited);

    /// <summary>
    /// VMF content-based hash code with recursion detection.
    /// </summary>
    int VmfHashCode(HashSet<object> visited);
}
