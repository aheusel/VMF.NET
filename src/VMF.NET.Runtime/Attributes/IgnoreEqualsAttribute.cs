// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Excludes this property from generated Equals() and GetHashCode() implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreEqualsAttribute : Attribute { }
