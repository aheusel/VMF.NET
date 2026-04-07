// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Adds documentation text to the generated API (emitted as XML doc comments).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
public sealed class DocAttribute : Attribute
{
    /// <summary>
    /// The documentation text.
    /// </summary>
    public string Value { get; }

    public DocAttribute(string value = "")
    {
        Value = value;
    }
}
