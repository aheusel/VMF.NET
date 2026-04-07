// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Implementation of a property change event.
/// </summary>
internal sealed class PropChangeImpl : IChangeInternal, IPropertyChange
{
    private readonly int _propertyId;

    public PropChangeImpl(
        IVObject obj,
        string propertyName,
        int propertyId,
        object? oldValue,
        object? newValue,
        string internalChangeInfo = "")
    {
        Object = obj;
        PropertyName = propertyName;
        _propertyId = propertyId;
        OldValue = oldValue;
        NewValue = newValue;
        InternalChangeInfo = internalChangeInfo;
        Timestamp = DateTime.UtcNow.Ticks;
    }

    public IVObject Object { get; }
    public string PropertyName { get; }
    public ChangeType ChangeType => ChangeType.Property;
    public long Timestamp { get; }
    public string InternalChangeInfo { get; }

    public object? OldValue { get; }
    public object? NewValue { get; }

    public bool IsUndoable
    {
        get
        {
            if (Object is IVObjectInternal voi)
            {
                var currentValue = voi.GetPropertyValueById(_propertyId);
                return Equals(currentValue, NewValue);
            }
            return false;
        }
    }

    public void Undo()
    {
        if (!IsUndoable)
            throw new InvalidOperationException($"Cannot undo change to '{PropertyName}'.");

        if (Object is IVObjectInternalModifiable modifiable)
        {
            modifiable.SetPropertyValueById(_propertyId, OldValue);
        }
    }

    public void Apply(IVObject target)
    {
        if (target is IVObjectInternalModifiable modifiable)
        {
            var targetPropId = ((IVObjectInternal)target).GetPropertyIdByName(PropertyName);
            modifiable.SetPropertyValueById(targetPropId, NewValue);
        }
    }

    IPropertyChange? IChange.PropertyChange => this;
    VListChangeEvent? IChange.ListChange => null;
}
