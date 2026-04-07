// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Implementation of <see cref="IReflect"/> for runtime reflection on VMF objects.
/// </summary>
public sealed class ReflectImpl : IReflect
{
    private readonly IVObjectInternal _parent;
    private IReadOnlyList<VmfProperty>? _properties;
    private IReadOnlyList<IAnnotation>? _annotations;
    private bool _staticOnly;

    public ReflectImpl(IVObjectInternal parent)
    {
        _parent = parent;
    }

    public void SetStaticOnly(bool staticOnly)
    {
        _staticOnly = staticOnly;
    }

    public IReadOnlyList<IAnnotation> Annotations()
    {
        // Annotations are populated by generated code
        return _annotations ??= [];
    }

    internal void SetAnnotations(IReadOnlyList<IAnnotation> annotations)
    {
        _annotations = annotations;
    }

    public IAnnotation? AnnotationByKey(string key)
    {
        return Annotations().FirstOrDefault(a => string.Equals(key, a.Key));
    }

    public IReadOnlyList<IAnnotation> AnnotationsByKey(string key)
    {
        return Annotations().Where(a => string.Equals(key, a.Key)).ToList();
    }

    public IReadOnlyList<VmfProperty> Properties()
    {
        if (_properties == null)
        {
            var names = _parent.GetPropertyNames();
            var props = new VmfProperty[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                props[i] = new VmfProperty(_parent, names[i], _staticOnly);
            }
            _properties = props;
        }
        return _properties;
    }

    public VmfProperty? PropertyByName(string name)
    {
        return Properties().FirstOrDefault(p => string.Equals(name, p.Name));
    }

    public VmfType Type()
    {
        return _parent.GetVmfType();
    }

    public IReadOnlyList<VmfType> AllTypes()
    {
        // This would be populated by generated code with all types in the model
        return [_parent.GetVmfType()];
    }
}
