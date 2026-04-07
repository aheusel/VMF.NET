// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Strategy for iterating over an object graph.
/// </summary>
public enum IterationStrategy
{
    /// <summary>
    /// Visits each node exactly once. References of the same node that are
    /// encountered later are skipped.
    /// </summary>
    UniqueNode,

    /// <summary>
    /// Visits each property of each node exactly once. The same node may be
    /// visited multiple times if referenced from different properties.
    /// </summary>
    UniqueProperty,

    /// <summary>
    /// Visits the containment tree only. Non-containment references are ignored.
    /// </summary>
    ContainmentTree
}
