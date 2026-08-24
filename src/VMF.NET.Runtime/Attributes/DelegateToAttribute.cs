// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Attributes;

/// <summary>
/// Delegates method calls to a custom behavior implementation class.
/// <para>
/// <b>On a method</b>, the generated implementation forwards that method to the behavior class.
/// </para>
/// <para>
/// <b>On the model interface</b>, it additionally makes the generated constructor call
/// <c>On&lt;TypeName&gt;Instantiated()</c> on the behavior — where <c>TypeName</c> is the
/// interface name with a leading <c>I</c> stripped, so <c>ICodeEntity</c> calls
/// <c>OnCodeEntityInstantiated</c>. The behavior class must declare that method. It is the
/// model's hook for running code when an object is created, such as registering a change
/// listener. A type-level attribute also supplies the behavior class for methods on that
/// interface that carry no <see cref="DelegateToAttribute"/> of their own.
/// </para>
/// <para>
/// Delegations are inherited: a subtype gets a body for a delegated method declared on a
/// supertype, and a re-declaration on the subtype overrides it. Of the type-level delegations in
/// a hierarchy, exactly one applies — the nearest.
/// </para>
/// <para>
/// The behavior class implements <see cref="IDelegatedBehavior{T}"/> once, at whichever model
/// type suits it — a supertype or even <see cref="IVObject"/> is fine, and every subtype that
/// satisfies it can use the same behavior. One instance is created per behavior class per object
/// and shared between the constructor hook and every delegated method, so a behavior may keep
/// state across calls; <c>SetCaller</c> is called once, when it is created.
/// </para>
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
