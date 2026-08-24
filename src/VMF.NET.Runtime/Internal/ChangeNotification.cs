// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Routes a change from the object it happened on to every <see cref="ChangesManager"/> that
/// can see it.
/// <para>
/// A manager sees a change on its own object, and on anything in the subtree it contains. Rather
/// than pushing the manager down into every descendant when it is created — which would have to
/// be maintained on every containment change, and would overwrite a manager a descendant already
/// had — this walks <em>up</em> the container chain when the change is fired. Reachability is the
/// container chain, so detaching an object stops its changes from reaching the old root with no
/// bookkeeping at all.
/// </para>
/// <para>
/// Whether a manager's listeners actually run is decided by <see cref="ChangesManager"/>: a
/// non-recursive listener only sees changes on the manager's own object. See
/// <c>devdoc/java-parity-roadmap.md</c>, "M4b design note".
/// </para>
/// </summary>
public static class ChangeNotification
{
    /// <summary>
    /// Containment is a tree, so the walk terminates on its own. The bound only stops a model
    /// that has managed to build a containment cycle from hanging the process.
    /// </summary>
    private const int MaxDepth = 100_000;

    /// <summary>Reports a scalar property change to every manager that can see it.</summary>
    public static void FireProperty(
        IVObject source,
        string propertyName,
        int propertyId,
        object? oldValue,
        object? newValue,
        string internalChangeInfo = "")
    {
        // A change to the object's own CONTAINER property is the echo of a containment change
        // that belongs to the container, so it is reported only where it happened. Java arrives
        // at the same place by ordering -- it fires before the child joins the parent's listener
        // graph -- and its fact records the outcome as "fired only locally in child".
        var managers = IsContainerProperty(source, propertyId) ? Own(source) : Collect(source);
        if (managers == null) return;

        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].FirePropertyChange(source, propertyName, propertyId, oldValue, newValue, internalChangeInfo);
        }
    }

    /// <summary>True if <paramref name="propertyId"/> is one of the object's container properties.</summary>
    internal static bool IsContainerProperty(IVObject source, int propertyId)
    {
        if (propertyId < 0 || source is not IVObjectInternal internalObj) return false;

        foreach (int id in internalObj.GetParentIndices())
        {
            if (id == propertyId) return true;
        }
        return false;
    }

    /// <summary>Just the object's own manager, if it has one.</summary>
    private static List<ChangesManager>? Own(IVObject source)
    {
        var manager = (source as IVObjectInternal)?.GetChangesManager();
        return manager == null ? null : [manager];
    }

    /// <summary>Reports a list change to every manager that can see it.</summary>
    public static void FireList(
        IVObject source,
        string propertyName,
        VListChangeEvent listChangeEvent,
        string internalChangeInfo = "")
    {
        var managers = Collect(source);
        if (managers == null) return;

        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].FireListChange(source, propertyName, listChangeEvent, internalChangeInfo);
        }
    }

    /// <summary>
    /// Collects the managers observing <paramref name="source"/>, nearest first. Returns null
    /// when there are none, which is the common case — no allocation happens until something
    /// is actually listening.
    /// </summary>
    private static List<ChangesManager>? Collect(IVObject source)
    {
        List<ChangesManager>? managers = null;
        IVObject? current = source;

        for (int depth = 0; current is IVObjectInternal internalObj && depth < MaxDepth; depth++)
        {
            var manager = internalObj.GetChangesManager();
            if (manager != null)
            {
                managers ??= new List<ChangesManager>(2);

                // The same manager can be reachable twice — it is stored on its own object and
                // may also have been handed to something further up. Report a change once.
                bool alreadySeen = false;
                for (int i = 0; i < managers.Count; i++)
                {
                    if (ReferenceEquals(managers[i], manager)) { alreadySeen = true; break; }
                }
                if (!alreadySeen) managers.Add(manager);
            }

            current = internalObj is IVObjectInternalModifiable modifiable ? modifiable.GetContainer() : null;
        }

        return managers;
    }
}
