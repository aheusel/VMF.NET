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
    /// The name of the opposite property on the container type.
    /// </summary>
    public string Opposite { get; }

    public ContainerAttribute(string opposite)
    {
        Opposite = opposite;
    }
}
