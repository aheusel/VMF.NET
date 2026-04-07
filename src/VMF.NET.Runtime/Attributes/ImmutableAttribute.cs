// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Marks a model type as immutable. Only read-only access is generated.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ImmutableAttribute : Attribute { }
