// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Implementation of <see cref="ITransaction"/>.
/// </summary>
internal sealed class TransactionImpl : ITransaction
{
    private readonly List<IChange> _changes;

    public TransactionImpl(IEnumerable<IChange> changes)
    {
        _changes = new List<IChange>(changes);
    }

    public IReadOnlyList<IChange> Changes => _changes;

    public bool IsUndoable => _changes.All(c => c.IsUndoable);

    public void Undo()
    {
        // Undo in reverse order
        for (int i = _changes.Count - 1; i >= 0; i--)
        {
            _changes[i].Undo();
        }
    }
}
