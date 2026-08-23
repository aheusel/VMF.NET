// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Defines the inverse side of a containment relationship.
/// The annotated property references the container (parent) object.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContainerAttribute : Attribute
{
    /// <summary>
    /// The name of the opposite property on the container type, or <c>null</c> when the
    /// container is not known at compile time (several unrelated properties, possibly on
    /// different types, may contain this type).
    /// </summary>
    public string? Opposite { get; }

    /// <summary>
    /// Declares a container back-reference with no single declared opposite.
    /// </summary>
    public ContainerAttribute()
    {
    }

    /// <summary>
    /// Declares a container back-reference with an explicit opposite property.
    /// </summary>
    public ContainerAttribute(string opposite)
    {
        Opposite = opposite;
    }
}
