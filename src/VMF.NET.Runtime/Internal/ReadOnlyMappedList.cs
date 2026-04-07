// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// A read-only list that lazily maps elements from a source list.
/// Used by generated read-only wrappers to present model-type collections
/// as read-only interface collections.
/// </summary>
public sealed class ReadOnlyMappedList<TSource, TTarget> : IReadOnlyList<TTarget>
{
    private readonly IList<TSource> _source;
    private readonly Func<TSource, TTarget> _map;

    public ReadOnlyMappedList(IList<TSource> source, Func<TSource, TTarget> map)
    {
        _source = source;
        _map = map;
    }

    public TTarget this[int index] => _map(_source[index]);
    public int Count => _source.Count;

    public IEnumerator<TTarget> GetEnumerator()
    {
        for (int i = 0; i < _source.Count; i++)
            yield return _map(_source[i]);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
