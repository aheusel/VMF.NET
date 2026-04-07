// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Declares a model interface as an external type (no code is generated for it).
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ExternalTypeAttribute : Attribute
{
    /// <summary>
    /// The namespace of the external type.
    /// </summary>
    public string Namespace { get; }

    public ExternalTypeAttribute(string @namespace)
    {
        Namespace = @namespace;
    }
}
