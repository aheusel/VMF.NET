// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using VMF.NET.Runtime.Internal;

namespace VMF.NET.Runtime;

/// <summary>
/// Provides reflective access to a VMF object's property.
/// Supports get/set/unset, type information, annotations, and change listeners.
/// </summary>
public sealed class VmfProperty
{
    private readonly IVObjectInternal _parent;
    private readonly int _propertyId;
    private readonly string _name;
    private readonly VmfType _type;
    private readonly bool _staticOnly;

    internal VmfProperty(IVObjectInternal parent, string name, bool staticOnly)
    {
        _parent = parent;
        _name = name;
        _staticOnly = staticOnly;
        _propertyId = parent.GetPropertyIdByName(name);

        var propertyTypes = parent.GetPropertyTypes();
        bool isModelType = propertyTypes[_propertyId] != -1;
        bool isListType = propertyTypes[_propertyId] == -2;

        if (isListType)
        {
            isModelType = false;
            foreach (var pId in parent.GetIndicesOfPropertiesWithModelElementTypes())
            {
                if (_propertyId == pId)
                {
                    isModelType = true;
                    break;
                }
            }
        }

        string typeName = parent.GetPropertyTypeNames()[_propertyId];
        _type = VmfType.Create(isModelType, isListType, false, typeName);
    }

    /// <summary>
    /// The name of this property.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The type of this property.
    /// </summary>
    public VmfType Type => _type;

    /// <summary>
    /// Indicates whether this property has been explicitly set (differs from default value).
    /// </summary>
    public bool IsSet
    {
        get
        {
            EnsureInstanceAccess();
            return _parent.IsSetById(_propertyId);
        }
    }

    /// <summary>
    /// Gets the current value of this property.
    /// </summary>
    public object? Get()
    {
        EnsureInstanceAccess();
        return _parent.GetPropertyValueById(_propertyId);
    }

    /// <summary>
    /// Sets this property to the specified value.
    /// </summary>
    public void Set(object? value)
    {
        EnsureInstanceAccess();
        if (_parent is IVObjectInternalModifiable modifiable)
        {
            modifiable.SetPropertyValueById(_propertyId, value);
        }
        else
        {
            throw new InvalidOperationException("Cannot modify unmodifiable object.");
        }
    }

    /// <summary>
    /// Resets this property to its default value.
    /// </summary>
    public void Unset()
    {
        EnsureInstanceAccess();
        if (_parent is IVObjectInternalModifiable modifiable)
        {
            modifiable.SetPropertyValueById(_propertyId, GetDefault());
        }
        else
        {
            throw new InvalidOperationException("Cannot modify unmodifiable object.");
        }
    }

    /// <summary>
    /// Returns the default value of this property.
    /// </summary>
    public object? GetDefault()
    {
        EnsureInstanceAccess();
        return _parent.GetDefaultValueById(_propertyId);
    }

    /// <summary>
    /// Returns the annotations on this property.
    /// </summary>
    public IReadOnlyList<IAnnotation> Annotations()
    {
        return _parent.GetPropertyAnnotationsById(_propertyId);
    }

    /// <summary>
    /// Returns the first annotation with the specified key, or null if not found.
    /// </summary>
    public IAnnotation? AnnotationByKey(string key)
    {
        return Annotations().FirstOrDefault(a => string.Equals(key, a.Key));
    }

    /// <summary>
    /// Adds a change listener for this specific property.
    /// Returns an <see cref="IDisposable"/> to unsubscribe.
    /// </summary>
    public IDisposable AddChangeListener(Action<IChange> listener)
    {
        EnsureInstanceAccess();
        return ((IVObject)_parent).Vmf().Changes().AddListener(change =>
        {
            if (string.Equals(Name, change.PropertyName))
            {
                listener(change);
            }
        }, recursive: false);
    }

    private void EnsureInstanceAccess()
    {
        if (_parent == null || _staticOnly)
        {
            throw new InvalidOperationException("Cannot access property without an instance.");
        }
    }
}
