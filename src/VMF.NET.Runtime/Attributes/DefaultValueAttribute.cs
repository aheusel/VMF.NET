// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Defines a custom default value for a property.
/// The value string is parsed according to the property type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
public sealed class VmfDefaultValueAttribute : Attribute
{
    /// <summary>
    /// The default value as a string (parsed at code generation time).
    /// </summary>
    public string Value { get; }

    public VmfDefaultValueAttribute(string value)
    {
        Value = value;
    }
}
