// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Indicates that only a public getter should be generated for this property.
/// Typically used with <see cref="InterfaceOnlyAttribute"/> types.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GetterOnlyAttribute : Attribute { }
