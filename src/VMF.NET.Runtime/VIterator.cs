// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections;
using VMF.NET.Runtime.Internal;

namespace VMF.NET.Runtime;

/// <summary>
/// Iterator that traverses an object graph depth-first.
/// Supports multiple iteration strategies and modification during traversal.
/// </summary>
public sealed class VIterator : IEnumerator<IVObject>, IEnumerable<IVObject>
{
    private readonly VmfIterator _iterator;

    private VIterator(VmfIterator iterator)
    {
        _iterator = iterator;
    }

    /// <summary>
    /// Creates an iterator for the specified root object using <see cref="IterationStrategy.UniqueNode"/>.
    /// </summary>
    public static VIterator Of(IVObject root)
    {
        return new VIterator(new VmfIterator((IVObjectInternal)root, null, IterationStrategy.UniqueNode));
    }

    /// <summary>
    /// Creates an iterator with the specified strategy.
    /// </summary>
    public static VIterator Of(IVObject root, IterationStrategy strategy)
    {
        return new VIterator(new VmfIterator((IVObjectInternal)root, null, strategy));
    }

    /// <summary>
    /// Creates an iterator with a traversal listener and strategy.
    /// </summary>
    public static VIterator Of(IVObject root, ITraversalListener? listener, IterationStrategy strategy)
    {
        return new VIterator(new VmfIterator((IVObjectInternal)root, listener, strategy));
    }

    /// <summary>The current element.</summary>
    public IVObject Current => _iterator.Current;

    object IEnumerator.Current => Current;

    public bool MoveNext() => _iterator.MoveNext();

    /// <summary>
    /// Replaces the last returned element with the specified object.
    /// </summary>
    public void Set(IVObject obj) => _iterator.Set(obj);

    /// <summary>
    /// Adds an object after the last returned element (only for list properties).
    /// </summary>
    public void Add(IVObject obj) => _iterator.Add(obj);

    /// <summary>
    /// Indicates whether adding at the current iterator position is supported.
    /// </summary>
    public bool IsAddSupported => _iterator.IsAddSupported;

    /// <summary>
    /// Returns this iterator as an enumerable for LINQ support.
    /// </summary>
    public IEnumerator<IVObject> GetEnumerator() => this;

    IEnumerator IEnumerable.GetEnumerator() => this;

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { }
}

/// <summary>
/// Internal iterator implementation for depth-first object graph traversal.
/// </summary>
internal sealed class VmfIterator
{
    private readonly Dictionary<object, object?> _identityMap = new(ReferenceEqualityComparer.Instance);
    private IVObjectInternal? _first;
    private IVObjectPropertyIterator _currentIterator;
    private IVObjectPropertyIterator? _prevIterator;
    private bool _usedCurrentIterator;
    private readonly Stack<IVObjectPropertyIterator> _iteratorStack = new();
    private readonly ITraversalListener? _traversalListener;
    private readonly IterationStrategy _strategy;
    private IVObject? _current;

    public VmfIterator(IVObjectInternal root, ITraversalListener? listener, IterationStrategy strategy)
    {
        _first = root;
        _traversalListener = listener;
        _strategy = strategy;
        _currentIterator = new VmfPropertyIterator(_identityMap, root, strategy);
    }

    public IVObject Current => _current ?? throw new InvalidOperationException("No current element.");

    public bool MoveNext()
    {
        if (_first != null)
        {
            var n = _first;
            if (n is not IImmutable)
            {
                _identityMap[n] = null;
            }
            _first = null;
            OnEnter(n);
            _current = n;
            return true;
        }

        var iter = GetCurrentIterator();
        if (!iter.HasNext)
            return false;

        var next = (IVObjectInternal)iter.Next();
        var identityObj = UnwrapIfReadOnly(next);

        if (!_identityMap.ContainsKey(identityObj))
        {
            if (identityObj is not IImmutable)
            {
                _identityMap[identityObj] = null;
            }
            _iteratorStack.Push(_currentIterator);
            _prevIterator = _currentIterator;
            OnEnter(next);
            _currentIterator = new VmfPropertyIterator(_identityMap, next, _strategy);
            _usedCurrentIterator = false;
        }

        _current = next;
        return true;
    }

    internal static object UnwrapIfReadOnly(object obj)
    {
        if (obj is IImmutable) return obj;
        if (obj is not IVObjectInternal n) return obj;
        return n.IsReadOnly ? n.GetMutableObject() : n;
    }

    public void Set(IVObject obj)
    {
        if (_first != null) throw new InvalidOperationException("Cannot replace root.");
        if (_usedCurrentIterator) _currentIterator.Set(obj);
        else _prevIterator?.Set(obj);
    }

    public void Add(IVObject obj)
    {
        if (_first != null) throw new InvalidOperationException("Cannot add to root.");
        if (_usedCurrentIterator) _currentIterator.Add(obj);
        else _prevIterator?.Add(obj);
    }

    public bool IsAddSupported =>
        _usedCurrentIterator ? _currentIterator.IsAddSupported :
        _prevIterator?.IsAddSupported ?? false;

    private IVObjectPropertyIterator GetCurrentIterator()
    {
        while (!_currentIterator.HasNext)
        {
            OnExit(_currentIterator.Object);
            if (_iteratorStack.Count > 0)
            {
                _currentIterator = _iteratorStack.Pop();
            }
            else
            {
                return EmptyIterator.Instance;
            }
        }
        return _currentIterator;
    }

    private void OnEnter(IVObject? obj)
    {
        if (_traversalListener == null) return;
        if (_traversalListener.IgnoreNullObjects && obj == null) return;
        _traversalListener.OnEnter(obj!);
    }

    private void OnExit(IVObject? obj)
    {
        if (_traversalListener == null) return;
        if (_traversalListener.IgnoreNullObjects && obj == null) return;
        _traversalListener.OnExit(obj!);
    }
}

/// <summary>
/// Interface for property-level iteration within a single object.
/// </summary>
internal interface IVObjectPropertyIterator
{
    bool HasNext { get; }
    IVObject Next();
    IVObject? Object { get; }
    void Set(IVObject obj);
    void Add(IVObject obj);
    bool IsAddSupported { get; }
}

internal sealed class EmptyIterator : IVObjectPropertyIterator
{
    public static readonly EmptyIterator Instance = new();
    public bool HasNext => false;
    public IVObject Next() => throw new InvalidOperationException("No elements.");
    public IVObject? Object => null;
    public void Set(IVObject obj) { }
    public void Add(IVObject obj) { }
    public bool IsAddSupported => false;
}

/// <summary>
/// Iterates over properties of a single VMF object, visiting contained model-type values.
/// </summary>
internal sealed class VmfPropertyIterator : IVObjectPropertyIterator
{
    private readonly IVObjectInternal _object;
    private int _index = -1;
    private System.Collections.IList? _currentList;
    private int _listIndex = -1;
    private readonly Dictionary<object, object?> _identityMap;
    private readonly IterationStrategy _strategy;

    public VmfPropertyIterator(
        Dictionary<object, object?> identityMap,
        IVObjectInternal obj,
        IterationStrategy strategy)
    {
        _identityMap = identityMap;
        _object = obj;
        _strategy = strategy;
    }

    public IVObject? Object => _object;

    private int[] GetPropertyIndices()
    {
        return _strategy == IterationStrategy.ContainmentTree
            ? _object.GetChildrenIndices()
            : _object.GetIndicesOfPropertiesWithModelTypeOrElementTypes();
    }

    public bool HasNext
    {
        get
        {
            if (_currentList != null)
            {
                if (HasNextListElement())
                    return true;
                _currentList = null;
            }

            var properties = GetPropertyIndices();
            int nextIndex = _index + 1;

            while (nextIndex < properties.Length)
            {
                int propIndex = properties[nextIndex];
                var val = _object.GetPropertyValueById(propIndex);

                if (val == null)
                {
                    nextIndex++;
                    continue;
                }

                if (val is System.Collections.IList list)
                {
                    bool hasNonEmpty = false;
                    foreach (var e in list)
                    {
                        if (e is IVObject vo && (_strategy != IterationStrategy.UniqueNode ||
                            !_identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(vo))))
                        {
                            hasNonEmpty = true;
                            break;
                        }
                    }

                    if (!hasNonEmpty)
                    {
                        nextIndex++;
                        continue;
                    }
                    return true;
                }

                if (_strategy == IterationStrategy.UniqueNode &&
                    _identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(val)))
                {
                    nextIndex++;
                    continue;
                }

                return true;
            }

            return false;
        }
    }

    public IVObject Next()
    {
        if (_currentList != null)
        {
            return NextListElement();
        }

        _index++;
        var properties = GetPropertyIndices();

        while (_index < properties.Length)
        {
            int propIndex = properties[_index];
            var val = _object.GetPropertyValueById(propIndex);

            if (val == null)
            {
                _index++;
                continue;
            }

            if (val is System.Collections.IList list)
            {
                // Check if the list has any relevant elements
                bool hasRelevant = false;
                foreach (var e in list)
                {
                    if (e is IVObject vo && (_strategy != IterationStrategy.UniqueNode ||
                        !_identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(vo))))
                    {
                        hasRelevant = true;
                        break;
                    }
                }
                if (!hasRelevant)
                {
                    _index++;
                    continue;
                }
                _currentList = list;
                _listIndex = -1;
                return NextListElement();
            }

            if (_strategy == IterationStrategy.UniqueNode &&
                _identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(val)))
            {
                _index++;
                continue;
            }

            return (IVObject)val;
        }

        throw new InvalidOperationException("No more elements.");
    }

    private IVObject NextListElement()
    {
        while (++_listIndex < _currentList!.Count)
        {
            if (_currentList[_listIndex] is not IVObject elem) continue;
            if (_strategy == IterationStrategy.UniqueNode &&
                _identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(elem)))
                continue;
            return elem;
        }
        throw new InvalidOperationException("No more list elements.");
    }

    private bool HasNextListElement()
    {
        for (int i = _listIndex + 1; i < _currentList!.Count; i++)
        {
            if (_currentList[i] is not IVObject elem) continue;
            if (_strategy == IterationStrategy.UniqueNode &&
                _identityMap.ContainsKey(VmfIterator.UnwrapIfReadOnly(elem)))
                continue;
            return true;
        }
        return false;
    }

    public void Set(IVObject obj)
    {
        if (_object.IsReadOnly) throw new InvalidOperationException("Cannot modify read-only object.");

        if (_currentList != null && _listIndex >= 0)
        {
            _currentList[_listIndex] = (object)obj;
        }
        else if (_object is IVObjectInternalModifiable modifiable)
        {
            var properties = GetPropertyIndices();
            if (_index >= 0 && _index < properties.Length)
                modifiable.SetPropertyValueById(properties[_index], obj);
        }
    }

    public void Add(IVObject obj)
    {
        if (_object.IsReadOnly) throw new InvalidOperationException("Cannot modify read-only object.");
        if (_currentList == null) throw new InvalidOperationException("Adding to non-list properties is not supported.");
        _currentList.Insert(_listIndex + 1, (object)obj);
    }

    public bool IsAddSupported => _currentList != null;
}
