// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Implementation of <see cref="IChanges"/>. Manages change recording,
/// listeners, transactions, and model versioning.
/// Port of Java ChangesImpl.
/// </summary>
public sealed class ChangesManager : IChanges
{
    private readonly IVObject _owner;
    private readonly List<Action<IChange>> _listeners = [];
    private readonly List<(Action<IChange> listener, bool recursive)> _listenerEntries = [];
    private bool _recording;
    private VList<IChange>? _allChanges;
    private VList<ITransaction>? _transactions;
    private readonly List<IChange> _currentTransaction = [];
    private bool _inTransaction;
    private long _versionNumber;

    public ChangesManager(IVObject owner)
    {
        _owner = owner;
    }

    public IDisposable AddListener(Action<IChange> listener)
    {
        return AddListener(listener, recursive: true);
    }

    public IDisposable AddListener(Action<IChange> listener, bool recursive)
    {
        _listeners.Add(listener);
        _listenerEntries.Add((listener, recursive));
        return new Subscription(() =>
        {
            _listeners.Remove(listener);
            _listenerEntries.RemoveAll(e => ReferenceEquals(e.listener, listener));
        });
    }

    public void Start()
    {
        _recording = true;
        _allChanges = [];
        _transactions = [];
        _currentTransaction.Clear();
        _inTransaction = false;
        _versionNumber = 0;
    }

    public void Stop()
    {
        if (_inTransaction)
        {
            PublishTransaction();
        }
        _recording = false;
    }

    public void StartTransaction()
    {
        _inTransaction = true;
        _currentTransaction.Clear();
    }

    public void PublishTransaction()
    {
        if (_currentTransaction.Count > 0)
        {
            var transaction = new TransactionImpl(_currentTransaction);
            _transactions?.Add(transaction);
            _currentTransaction.Clear();
        }
        _inTransaction = false;
    }

    public VList<IChange> All()
    {
        return _allChanges ??= [];
    }

    public VList<ITransaction> Transactions()
    {
        return _transactions ??= [];
    }

    public void Clear()
    {
        _allChanges?.Clear();
        _transactions?.Clear();
        _currentTransaction.Clear();
    }

    public IModelVersion ModelVersion()
    {
        return new ModelVersionImpl(DateTime.UtcNow.Ticks, _versionNumber);
    }

    public bool IsModelVersioningEnabled => _recording;

    /// <summary>
    /// Called by generated code to notify about a property change.
    /// </summary>
    public void FirePropertyChange(
        IVObject obj,
        string propertyName,
        int propertyId,
        object? oldValue,
        object? newValue,
        string internalChangeInfo = "")
    {
        var change = new PropChangeImpl(obj, propertyName, propertyId, oldValue, newValue, internalChangeInfo);
        ProcessChange(change);
    }

    /// <summary>
    /// Called by generated code to notify about a list change.
    /// </summary>
    public void FireListChange(
        IVObject obj,
        string propertyName,
        VListChangeEvent listChangeEvent,
        string internalChangeInfo = "")
    {
        var change = new ListChangeImpl(obj, propertyName, listChangeEvent, internalChangeInfo);
        ProcessChange(change);
    }

    private void ProcessChange(IChange change)
    {
        if (_recording)
        {
            _allChanges?.Add(change);
            _versionNumber++;

            if (_inTransaction)
            {
                _currentTransaction.Add(change);
            }
        }

        // Notify listeners regardless of recording state
        foreach (var listener in _listeners)
        {
            listener(change);
        }
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}
