// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Core;

/// <summary>
/// Type of containment relationship.
/// </summary>
public enum ContainmentType
{
    /// <summary>No containment.</summary>
    None,
    /// <summary>This property contains (owns) the referenced object(s).</summary>
    Contained,
    /// <summary>This property references the container (parent).</summary>
    Container
}

/// <summary>
/// Classification of a property's type.
/// </summary>
public enum PropType
{
    /// <summary>A C# value type (int, bool, etc.).</summary>
    Primitive,
    /// <summary>A reference type (string, model types, external types).</summary>
    Class,
    /// <summary>A collection type (IList&lt;T&gt; or VList&lt;T&gt;).</summary>
    Collection
}
