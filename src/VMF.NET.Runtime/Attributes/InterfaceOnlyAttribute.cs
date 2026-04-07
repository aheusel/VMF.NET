// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Marks a model type as interface-only. No implementation class is generated;
/// the type cannot be instantiated directly.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class InterfaceOnlyAttribute : Attribute { }
