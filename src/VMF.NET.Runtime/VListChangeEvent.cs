// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// The type of change in a VList.
/// </summary>
public enum VListChangeType
{
    /// <summary>Elements were added.</summary>
    Add,
    /// <summary>Elements were removed.</summary>
    Remove,
    /// <summary>An element was replaced (set).</summary>
    Set
}

/// <summary>
/// Describes a change event on a <see cref="VList{T}"/>.
/// </summary>
public sealed class VListChangeEvent
{
    private VListChangeEvent(
        VListChangeType changeType,
        IReadOnlyList<object?> added,
        IReadOnlyList<object?> removed,
        int index,
        string? eventInfo,
        System.Collections.IList? source)
    {
        ChangeType = changeType;
        Added = added;
        Removed = removed;
        Index = index;
        EventInfo = eventInfo;
        Source = source;
    }

    /// <summary>The type of change.</summary>
    public VListChangeType ChangeType { get; }

    /// <summary>Elements that were added (empty for remove events).</summary>
    public IReadOnlyList<object?> Added { get; }

    /// <summary>Elements that were removed (empty for add events).</summary>
    public IReadOnlyList<object?> Removed { get; }

    /// <summary>The index at which the change occurred.</summary>
    public int Index { get; }

    /// <summary>Optional event info metadata.</summary>
    public string? EventInfo { get; }

    /// <summary>
    /// The list this change happened on (Java's <c>evt.source()</c>), so a listener can reach the
    /// list it is observing — including to modify it while handling the event. Null for an event
    /// constructed without one.
    /// </summary>
    public System.Collections.IList? Source { get; }

    /// <summary>Whether elements were added.</summary>
    public bool WasAdded => ChangeType == VListChangeType.Add || ChangeType == VListChangeType.Set;

    /// <summary>Whether elements were removed.</summary>
    public bool WasRemoved => ChangeType == VListChangeType.Remove || ChangeType == VListChangeType.Set;

    /// <summary>Whether an element was replaced.</summary>
    public bool WasSet => ChangeType == VListChangeType.Set;

    public static VListChangeEvent CreateAddEvent(IReadOnlyList<object?> added, int index,
        string? eventInfo = null, System.Collections.IList? source = null)
    {
        return new VListChangeEvent(VListChangeType.Add, added, Array.Empty<object?>(), index, eventInfo, source);
    }

    public static VListChangeEvent CreateRemoveEvent(IReadOnlyList<object?> removed, int index,
        string? eventInfo = null, System.Collections.IList? source = null)
    {
        return new VListChangeEvent(VListChangeType.Remove, Array.Empty<object?>(), removed, index, eventInfo, source);
    }

    public static VListChangeEvent CreateSetEvent(object? oldValue, object? newValue, int index,
        string? eventInfo = null, System.Collections.IList? source = null)
    {
        return new VListChangeEvent(VListChangeType.Set, [newValue], [oldValue], index, eventInfo, source);
    }

    public override string ToString()
    {
        return $"VListChangeEvent[type={ChangeType}, index={Index}, added={Added.Count}, removed={Removed.Count}]";
    }
}
