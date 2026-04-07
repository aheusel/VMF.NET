// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Defines a bidirectional cross-reference between two properties.
/// Unlike containment, neither side owns the other.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RefersAttribute : Attribute
{
    /// <summary>
    /// The name of the opposite property on the referenced type.
    /// </summary>
    public string Opposite { get; }

    public RefersAttribute(string opposite)
    {
        Opposite = opposite;
    }
}
