// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Adds custom metadata to model types and properties, queryable via the reflection API.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class VmfAnnotationAttribute : Attribute
{
    /// <summary>
    /// The annotation key (used to group annotations into categories).
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// The annotation value.
    /// </summary>
    public string Value { get; }

    public VmfAnnotationAttribute(string value)
    {
        Value = value;
    }
}
