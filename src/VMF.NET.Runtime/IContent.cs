// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Content API for object graph traversal, cloning, and equality.
/// Accessed via <c>obj.Vmf().Content()</c>.
/// </summary>
public interface IContent
{
    /// <summary>
    /// Returns an iterator that traverses the object graph depth-first
    /// using the <see cref="IterationStrategy.UniqueProperty"/> strategy.
    /// </summary>
    VIterator Iterator();

    /// <summary>
    /// Returns an iterator that traverses the object graph depth-first
    /// using the specified iteration strategy.
    /// </summary>
    VIterator Iterator(IterationStrategy strategy);

    /// <summary>
    /// Returns all elements of the object graph as an enumerable (depth-first)
    /// using the <see cref="IterationStrategy.UniqueProperty"/> strategy.
    /// </summary>
    IEnumerable<IVObject> Stream();

    /// <summary>
    /// Returns all elements of the object graph as an enumerable (depth-first)
    /// using the specified iteration strategy.
    /// </summary>
    IEnumerable<IVObject> Stream(IterationStrategy strategy);

    /// <summary>
    /// Returns all elements of the object graph that are assignable to <typeparamref name="T"/>.
    /// </summary>
    IEnumerable<T> Stream<T>() where T : IVObject;

    /// <summary>
    /// Returns all elements of the object graph that are assignable to <typeparamref name="T"/>
    /// using the specified iteration strategy.
    /// </summary>
    IEnumerable<T> Stream<T>(IterationStrategy strategy) where T : IVObject;

    /// <summary>
    /// Returns a deep copy of this object.
    /// </summary>
    T DeepCopy<T>();

    /// <summary>
    /// Returns a shallow copy of this object.
    /// </summary>
    T ShallowCopy<T>();

    /// <summary>
    /// VMF content-based equality comparison.
    /// </summary>
    bool ContentEquals(object? other);

    /// <summary>
    /// VMF content-based hash code.
    /// </summary>
    int ContentHashCode();
}
