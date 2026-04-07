// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Delegates method calls to a custom behavior implementation class.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public sealed class DelegateToAttribute : Attribute
{
    /// <summary>
    /// The type implementing the delegated behavior.
    /// </summary>
    public Type BehaviorType { get; }

    public DelegateToAttribute(Type behaviorType)
    {
        BehaviorType = behaviorType;
    }
}
