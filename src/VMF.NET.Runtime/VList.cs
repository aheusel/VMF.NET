// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace VMF.NET.Runtime;

/// <summary>
/// Observable list with VMF-specific hooks for containment management,
/// cross-reference management, and element change callbacks.
/// Extends <see cref="ObservableCollection{T}"/> to inherit
/// <see cref="INotifyCollectionChanged"/> for UI data binding.
/// </summary>
public class VList<T> : ObservableCollection<T>
{
    private readonly List<Action<VListChangeEvent>> _changeListeners = [];
    private Action<T, T?>? _onElementAdded;
    private Action<T, T?>? _onElementRemoved;
    private string? _eventInfo;

    public VList() { }

    public VList(IEnumerable<T> items) : base(items) { }

    /// <summary>
    /// Optional metadata string attached to change events (used for containment/cross-ref info).
    /// </summary>
    public string? EventInfo
    {
        get => _eventInfo;
        set => _eventInfo = value;
    }

    /// <summary>
    /// Registers a callback invoked when an element is added.
    /// Parameters: (addedElement, eventInfo).
    /// </summary>
    public void SetOnElementAdded(Action<T, T?>? callback) => _onElementAdded = callback;

    /// <summary>
    /// Registers a callback invoked when an element is removed.
    /// Parameters: (removedElement, eventInfo).
    /// </summary>
    public void SetOnElementRemoved(Action<T, T?>? callback) => _onElementRemoved = callback;

    /// <summary>
    /// Adds a change listener. Returns an <see cref="IDisposable"/> to unsubscribe.
    /// </summary>
    public IDisposable AddChangeListener(Action<VListChangeEvent> listener)
    {
        _changeListeners.Add(listener);
        return new Subscription(() => _changeListeners.Remove(listener));
    }

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        var evt = VListChangeEvent.CreateAddEvent([item], index, _eventInfo);
        _onElementAdded?.Invoke(item, default);
        FireChangeEvent(evt);
    }

    protected override void RemoveItem(int index)
    {
        var removed = this[index];
        base.RemoveItem(index);
        var evt = VListChangeEvent.CreateRemoveEvent([removed], index, _eventInfo);
        _onElementRemoved?.Invoke(removed, default);
        FireChangeEvent(evt);
    }

    protected override void SetItem(int index, T item)
    {
        var old = this[index];
        base.SetItem(index, item);
        var evt = VListChangeEvent.CreateSetEvent(old, item, index, _eventInfo);
        if (old != null) _onElementRemoved?.Invoke(old, default);
        _onElementAdded?.Invoke(item, default);
        FireChangeEvent(evt);
    }

    protected override void ClearItems()
    {
        var removed = new List<T>(this);
        base.ClearItems();
        for (int i = removed.Count - 1; i >= 0; i--)
        {
            var evt = VListChangeEvent.CreateRemoveEvent([removed[i]], i, _eventInfo);
            _onElementRemoved?.Invoke(removed[i], default);
            FireChangeEvent(evt);
        }
    }

    /// <summary>
    /// Appends several elements as a single change: listeners see one event carrying all of
    /// them, not one event per element.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));

        var added = items as IList<T> ?? items.ToList();
        if (added.Count == 0) return;

        CheckReentrancy();

        int index = Items.Count;
        for (int i = 0; i < added.Count; i++)
        {
            Items.Add(added[i]);
        }

        var elements = new object?[added.Count];
        for (int i = 0; i < added.Count; i++)
        {
            elements[i] = added[i];
            _onElementAdded?.Invoke(added[i], default);
        }

        RaiseBatchCollectionChanged();
        FireChangeEvent(VListChangeEvent.CreateAddEvent(elements, index, _eventInfo));
    }

    /// <summary>
    /// Removes the elements at the given indices as a single change: listeners see one event
    /// carrying every removed element. Indices may be given in any order; duplicates are
    /// ignored. Nothing is removed unless all indices are in range.
    /// </summary>
    public void RemoveAll(params int[] indices)
    {
        if (indices is null) throw new ArgumentNullException(nameof(indices));
        if (indices.Length == 0) return;

        // Validate before mutating, so a bad index cannot leave a half-applied removal behind.
        var ordered = indices.Distinct().OrderBy(i => i).ToArray();
        foreach (int index in ordered)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(indices), index, $"Index is out of range for a list of {Items.Count} elements.");
            }
        }

        CheckReentrancy();

        // Collected in ascending index order so the event lists them as the list held them,
        // but removed descending so the remaining indices stay valid.
        var removed = new object?[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
        {
            removed[i] = Items[ordered[i]];
        }
        for (int i = ordered.Length - 1; i >= 0; i--)
        {
            var element = Items[ordered[i]];
            Items.RemoveAt(ordered[i]);
            _onElementRemoved?.Invoke(element, default);
        }

        RaiseBatchCollectionChanged();
        FireChangeEvent(VListChangeEvent.CreateRemoveEvent(removed, ordered[0], _eventInfo));
    }

    /// <summary>
    /// A batch touches several positions at once. <c>ObservableCollection</c> consumers such as
    /// <c>CollectionView</c> reject a multi-element Add or Remove notification, so a batch is
    /// reported to <see cref="INotifyCollectionChanged"/> as a Reset. VMF's own change event
    /// still carries the individual elements.
    /// </summary>
    private void RaiseBatchCollectionChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void FireChangeEvent(VListChangeEvent evt)
    {
        foreach (var listener in _changeListeners)
        {
            listener(evt);
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
