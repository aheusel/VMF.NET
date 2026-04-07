// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Behavior API for accessing and setting delegated behavior implementations.
/// </summary>
public interface IBehavior<T> where T : IVObject
{
    /// <summary>Returns the current delegated behavior.</summary>
    IDelegatedBehavior<T> Get();

    /// <summary>Sets the delegated behavior.</summary>
    void Set(IDelegatedBehavior<T> behavior);
}

/// <summary>
/// Interface for custom behavior delegation on VMF objects.
/// </summary>
public interface IDelegatedBehavior<T> where T : IVObject
{
    /// <summary>
    /// Sets the caller that delegates to this behavior.
    /// Called by VMF after initialization.
    /// </summary>
    void SetCaller(T caller) { }
}

/// <summary>
/// Base class for delegated behavior implementations that stores the caller reference.
/// </summary>
public abstract class DelegatedBehaviorBase<T> : IDelegatedBehavior<T> where T : IVObject
{
    /// <summary>
    /// The caller currently associated with this behavior, or null.
    /// </summary>
    protected T? Caller { get; private set; }

    public void SetCaller(T caller)
    {
        Caller = caller;
    }
}
