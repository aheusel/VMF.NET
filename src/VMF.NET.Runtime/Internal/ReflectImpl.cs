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
        // Read on demand from the generated type metadata, as Java's ReflectImpl does. There is
        // deliberately no setter: an earlier SetAnnotations existed and nothing ever called it,
        // which is why this returned empty for every type.
        return _annotations ??= _parent.GetAnnotations();
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
        // Every model type in this object's model. Each generated model registers its types on
        // assembly load, and a model is one namespace, so the namespace is the model boundary.
        var name = _parent.GetVmfType().Name;
        int lastDot = name.LastIndexOf('.');
        if (lastDot <= 0) return [_parent.GetVmfType()];

        var all = VmfTypeRegistry.AllInNamespace(name.Substring(0, lastDot));
        return all.Count > 0 ? all : [_parent.GetVmfType()];
    }
}
