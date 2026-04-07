// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Reflection API for runtime introspection of VMF types, properties, and annotations.
/// Accessed via <c>obj.Vmf().Reflect()</c>.
/// </summary>
public interface IReflect
{
    /// <summary>
    /// Returns the annotations on this type.
    /// </summary>
    IReadOnlyList<IAnnotation> Annotations();

    /// <summary>
    /// Returns the first annotation with the specified key, or null if none found.
    /// </summary>
    IAnnotation? AnnotationByKey(string key);

    /// <summary>
    /// Returns all annotations with the specified key.
    /// </summary>
    IReadOnlyList<IAnnotation> AnnotationsByKey(string key);

    /// <summary>
    /// Returns all properties of this object.
    /// </summary>
    IReadOnlyList<VmfProperty> Properties();

    /// <summary>
    /// Returns the property with the specified name, or null if not found.
    /// </summary>
    VmfProperty? PropertyByName(string name);

    /// <summary>
    /// Returns the VMF type information for this object.
    /// </summary>
    VmfType Type();

    /// <summary>
    /// Returns all types in this VMF model.
    /// </summary>
    IReadOnlyList<VmfType> AllTypes();
}
