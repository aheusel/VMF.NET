// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Core;

/// <summary>
/// Equals strategy for generated code. Mirrors the EqualsType enum in Runtime.
/// </summary>
public enum EqualsStrategy
{
    /// <summary>Reference (identity) equality.</summary>
    Instance,
    /// <summary>Content-based equality for containment and external properties.</summary>
    ContainmentAndExternal,
    /// <summary>Content-based equality for all properties.</summary>
    All
}

/// <summary>
/// Model-wide configuration from [VmfModel] attribute.
/// </summary>
public sealed class ModelConfig
{
    public EqualsStrategy EqualsDefault { get; set; } = EqualsStrategy.Instance;

    public static ModelConfig Default => new();
}
