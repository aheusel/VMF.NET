// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// VMF API accessor. Gives access to content, changes, reflection, and behavior APIs.
/// </summary>
public interface IVmf
{
    /// <summary>
    /// The content API for object graph operations.
    /// </summary>
    IContent Content { get; }

    /// <summary>
    /// The changes API for change tracking, undo/redo.
    /// </summary>
    IChanges Changes { get; }

    /// <summary>
    /// The reflection API for runtime type introspection.
    /// </summary>
    IReflect Reflect { get; }

    /// <summary>
    /// Returns the behavior API for delegation support.
    /// <para>
    /// Stays a method, unlike its siblings above: a property cannot be generic.
    /// </para>
    /// </summary>
    IBehavior<T> Behavior<T>() where T : IVObject;
}
