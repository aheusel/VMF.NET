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
    /// Returns the content API for object graph operations.
    /// </summary>
    IContent Content();

    /// <summary>
    /// Returns the changes API for change tracking, undo/redo.
    /// </summary>
    IChanges Changes();

    /// <summary>
    /// Returns the reflection API for runtime type introspection.
    /// </summary>
    IReflect Reflect();

    /// <summary>
    /// Returns the behavior API for delegation support.
    /// </summary>
    IBehavior<T> Behavior<T>() where T : IVObject;
}
