// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Defines the order of this property for traversal and reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyOrderAttribute : Attribute
{
    /// <summary>
    /// The property order index (lower values come first).
    /// </summary>
    public int Index { get; }

    public PropertyOrderAttribute(int index)
    {
        Index = index;
    }
}
