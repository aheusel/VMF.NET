// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Configures a custom equals/hashCode implementation for a model type.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class VmfEqualsAttribute : Attribute
{
    /// <summary>
    /// The equality strategy.
    /// </summary>
    public EqualsType Value { get; }

    public VmfEqualsAttribute(EqualsType value = EqualsType.ContainmentAndExternal)
    {
        Value = value;
    }
}
