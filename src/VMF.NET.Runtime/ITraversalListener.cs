// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Listener for object graph traversal events.
/// </summary>
public interface ITraversalListener
{
    /// <summary>
    /// Called when the traversal enters an object node.
    /// </summary>
    void OnEnter(IVObject obj);

    /// <summary>
    /// Called when the traversal exits an object node.
    /// </summary>
    void OnExit(IVObject obj);

    /// <summary>
    /// Indicates whether null objects should be ignored during traversal.
    /// </summary>
    bool IgnoreNullObjects => true;

    /// <summary>
    /// Traverses the specified object graph using the default strategy.
    /// </summary>
    static void Traverse(IVObject root, ITraversalListener listener)
    {
        Traverse(root, listener, IterationStrategy.UniqueNode);
    }

    /// <summary>
    /// Traverses the specified object graph using the specified strategy.
    /// </summary>
    static void Traverse(IVObject root, ITraversalListener listener, IterationStrategy strategy)
    {
        var it = VIterator.Of(root, listener, strategy);
        while (it.MoveNext()) { }
    }
}
