// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// A collection of atomic changes that can be undone together.
/// Transactions describe higher-level model edits.
/// </summary>
public interface ITransaction
{
    /// <summary>
    /// The atomic changes that are part of this transaction.
    /// </summary>
    IReadOnlyList<IChange> Changes { get; }

    /// <summary>
    /// Indicates whether this transaction (and all contained changes) can be undone.
    /// </summary>
    bool IsUndoable { get; }

    /// <summary>
    /// Undoes this transaction by reverting all contained changes in reverse order.
    /// </summary>
    void Undo();
}
