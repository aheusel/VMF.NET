// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Content API for object graph traversal, cloning, and equality.
/// Accessed via <c>obj.VMF.Content</c>.
/// </summary>
public interface IContent
{
    /// <summary>
    /// Returns an iterator that traverses the object graph depth-first
    /// using the <see cref="IterationStrategy.UniqueNode"/> strategy.
    /// </summary>
    /// <summary>
    /// A cursor over the object graph, depth-first, using
    /// <see cref="IterationStrategy.UniqueNode"/>.
    /// <para>
    /// Use this only when you need to <b>modify the graph while traversing it</b> — the cursor
    /// exposes <c>Set</c>, <c>Add</c> and <c>IsAddSupported</c>, which no sequence can express.
    /// For reading, use <see cref="Traverse()"/> and LINQ.
    /// </para>
    /// </summary>
    VIterator Cursor();

    /// <summary>
    /// A cursor over the object graph, depth-first, using the given strategy.
    /// </summary>
    VIterator Cursor(IterationStrategy strategy);

    /// <summary>
    /// The object graph reachable from here — <b>this object first</b>, then depth-first through
    /// every model-typed property, visiting each object once.
    /// <para>
    /// Note this is a <b>graph</b> walk, not a containment tree: it follows <c>[Refers]</c>
    /// cross-references and <c>[Container]</c> back-pointers as well as containment, so it can
    /// reach objects that are not descendants of this one. Pass
    /// <see cref="IterationStrategy.ContainmentTree"/> for containment alone.
    /// </para>
    /// <para>
    /// Compose with LINQ: <c>Traverse().OfType&lt;Node&gt;()</c> selects by type, <c>.Count()</c>
    /// counts, and so on. The sequence is lazy and may be enumerated repeatedly.
    /// </para>
    /// </summary>
    IEnumerable<IVObject> Traverse();

    /// <summary>
    /// The object graph reachable from here, this object first, using the given strategy.
    /// <see cref="IterationStrategy.ContainmentTree"/> restricts it to containment, i.e. to this
    /// object and its descendants.
    /// </summary>
    IEnumerable<IVObject> Traverse(IterationStrategy strategy);

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
