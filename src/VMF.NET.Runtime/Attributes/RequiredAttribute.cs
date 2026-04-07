// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Marks a property as required. Required properties must be set via the builder.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class VmfRequiredAttribute : Attribute { }
