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
    /// The name of the opposite property on the contained type, or <c>null</c> for an
    /// opposite-less containment (the contained type declares no back-reference; its parent
    /// is tracked internally by the implementation).
    /// </summary>
    public string? Opposite { get; }

    /// <summary>
    /// Declares containment with no opposite (back-reference) on the contained type.
    /// </summary>
    public ContainsAttribute()
    {
    }

    /// <summary>
    /// Declares containment with an explicit opposite property on the contained type.
    /// </summary>
    public ContainsAttribute(string opposite)
    {
        Opposite = opposite;
    }
}
