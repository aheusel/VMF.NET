// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Implementation of a list change event.
/// </summary>
internal sealed class ListChangeImpl : IChangeInternal
{
    private readonly VListChangeEvent _listChangeEvent;

    public ListChangeImpl(
        IVObject obj,
        string propertyName,
        VListChangeEvent listChangeEvent,
        string internalChangeInfo = "")
    {
        Object = obj;
        PropertyName = propertyName;
        _listChangeEvent = listChangeEvent;
        InternalChangeInfo = internalChangeInfo;
        Timestamp = DateTime.UtcNow.Ticks;
    }

    public IVObject Object { get; }
    public string PropertyName { get; }
    public ChangeType ChangeType => ChangeType.List;
    public long Timestamp { get; }
    public string InternalChangeInfo { get; }

    public bool IsUndoable => true;

    public void Undo()
    {
        if (Object is not IVObjectInternal voi) return;
        var propId = voi.GetPropertyIdByName(PropertyName);
        var list = voi.GetPropertyValueById(propId);
        if (list == null) return;

        // Get the list as dynamic to call list methods
        dynamic dynList = list;
        switch (_listChangeEvent.ChangeType)
        {
            case VListChangeType.Add:
                // Undo add by removing
                for (int i = _listChangeEvent.Added.Count - 1; i >= 0; i--)
                {
                    dynList.RemoveAt(_listChangeEvent.Index + i);
                }
                break;

            case VListChangeType.Remove:
                // Undo remove by re-inserting
                for (int i = 0; i < _listChangeEvent.Removed.Count; i++)
                {
                    dynList.Insert(_listChangeEvent.Index + i, (dynamic?)_listChangeEvent.Removed[i]);
                }
                break;

            case VListChangeType.Set:
                // Undo set by restoring old value
                if (_listChangeEvent.Removed.Count > 0)
                {
                    dynList[_listChangeEvent.Index] = (dynamic?)_listChangeEvent.Removed[0];
                }
                break;
        }
    }

    public void Apply(IVObject target)
    {
        if (target is not IVObjectInternal voi) return;
        var propId = voi.GetPropertyIdByName(PropertyName);
        var list = voi.GetPropertyValueById(propId);
        if (list == null) return;

        dynamic dynList = list;
        switch (_listChangeEvent.ChangeType)
        {
            case VListChangeType.Add:
                for (int i = 0; i < _listChangeEvent.Added.Count; i++)
                {
                    dynList.Insert(_listChangeEvent.Index + i, (dynamic?)_listChangeEvent.Added[i]);
                }
                break;

            case VListChangeType.Remove:
                for (int i = _listChangeEvent.Removed.Count - 1; i >= 0; i--)
                {
                    dynList.RemoveAt(_listChangeEvent.Index + i);
                }
                break;

            case VListChangeType.Set:
                if (_listChangeEvent.Added.Count > 0)
                {
                    dynList[_listChangeEvent.Index] = (dynamic?)_listChangeEvent.Added[0];
                }
                break;
        }
    }

    IPropertyChange? IChange.PropertyChange => null;
    VListChangeEvent? IChange.ListChange => _listChangeEvent;
}
