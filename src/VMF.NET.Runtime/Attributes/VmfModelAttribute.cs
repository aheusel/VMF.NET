// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Configures a VMF model type. At most one type per model should carry this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class VmfModelAttribute : Attribute
{
    /// <summary>
    /// The equality strategy for generated equals/hashCode.
    /// </summary>
    public EqualsType Equality { get; set; } = EqualsType.Instance;
}

/// <summary>
/// Equality strategy for VMF model types.
/// </summary>
public enum EqualsType
{
    /// <summary>Reference (identity) equality. This is the default.</summary>
    Instance,

    /// <summary>Content-based equality considering containment and external properties.</summary>
    ContainmentAndExternal,

    /// <summary>Content-based equality considering all properties.</summary>
    All
}
