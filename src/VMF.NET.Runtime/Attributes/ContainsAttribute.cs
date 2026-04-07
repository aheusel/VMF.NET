// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Defines a containment relationship. The annotated property owns the contained objects.
/// When a contained object is added to this property, it is automatically removed from
/// any previous container.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ContainsAttribute : Attribute
{
    /// <summary>
    /// The name of the opposite property on the contained type.
    /// </summary>
    public string Opposite { get; }

    public ContainsAttribute(string opposite)
    {
        Opposite = opposite;
    }
}
