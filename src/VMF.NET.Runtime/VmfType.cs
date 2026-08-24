// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using VMF.NET.Runtime.Internal;

namespace VMF.NET.Runtime;

/// <summary>
/// Represents a VMF type with metadata about whether it is a model type, list type, etc.
/// Named <c>VmfType</c> to avoid conflict with <see cref="System.Type"/>.
/// </summary>
public sealed class VmfType : IEquatable<VmfType>
{
    private List<VmfType>? _superTypes;

    // Static reflection is instance reflection over a throwaway object, exactly as in Java --
    // Type.reflect() there builds a prototype and disables the instance-dependent operations.
    // Java has to go through Class.forName because its Type carries only a name; the generator
    // can hand us a factory directly, so no runtime type resolution is involved.
    private readonly Func<IVObject>? _prototypeFactory;
    private IVObject? _prototype;

    private VmfType(bool isModelType, bool isListType, bool isInterfaceOnly, string name,
                    Func<IVObject>? prototypeFactory)
    {
        IsModelType = isModelType;
        IsListType = isListType;
        IsInterfaceOnly = isInterfaceOnly;
        Name = name;
        _prototypeFactory = prototypeFactory;
    }

    public static VmfType Create(bool isModelType, bool isListType, bool isInterfaceOnly, string name)
    {
        return new VmfType(isModelType, isListType, isInterfaceOnly, name, null);
    }

    /// <summary>
    /// Creates a type that supports static reflection. <paramref name="prototypeFactory"/> builds
    /// the throwaway instance used to answer <see cref="Reflect"/> and <see cref="SuperTypes"/>.
    /// </summary>
    public static VmfType Create(bool isModelType, bool isListType, bool isInterfaceOnly, string name,
                                 Func<IVObject> prototypeFactory)
    {
        return new VmfType(isModelType, isListType, isInterfaceOnly, name, prototypeFactory);
    }

    /// <summary>
    /// The full name of this type (including namespace).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Indicates whether this type is a VMF model type.
    /// </summary>
    public bool IsModelType { get; }

    /// <summary>
    /// Indicates whether this type is a list type.
    /// </summary>
    public bool IsListType { get; }

    /// <summary>
    /// Indicates whether this type is interface-only (not instantiable).
    /// </summary>
    public bool IsInterfaceOnly { get; }

    /// <summary>
    /// Returns the element type name if this is a list type.
    /// </summary>
    public string? GetElementTypeName()
    {
        if (!IsListType) return null;

        int firstIdx = Name.IndexOf('<');
        if (firstIdx < 1) return null;

        int lastIdx = Name.LastIndexOf('>');
        if (lastIdx <= firstIdx) return null;

        return Name.Substring(firstIdx + 1, lastIdx - firstIdx - 1);
    }

    /// <summary>
    /// Returns the reflection API of this type, without an instance. Reading metadata works;
    /// anything needing an object -- get, set, unset, is-set, listeners -- throws.
    /// </summary>
    public IReflect Reflect()
    {
        var self = VmfTypeRegistry.Lookup(Name) ?? this;
        var reflect = (ReflectImpl)self.Prototype().Vmf().Reflect();
        reflect.SetStaticOnly(true);
        return reflect;
    }

    /// <summary>
    /// Returns the super types of this type. Empty for anything that is not a model type,
    /// including list types whatever their element type.
    /// </summary>
    public IReadOnlyList<VmfType> SuperTypes()
    {
        if (_superTypes != null) return _superTypes;

        _superTypes = [];

        if (IsModelType && !IsListType)
        {
            // A type reached through a property carries only a name -- VmfProperty builds it from
            // the parent's metadata -- so resolve through the registry to get the registered type
            // with its prototype factory. Java resolves the same way, via Class.forName.
            var self = VmfTypeRegistry.Lookup(Name) ?? this;

            foreach (var name in ((IVObjectInternal)self.Prototype()).GetSuperTypeNames())
            {
                _superTypes.Add(VmfTypeRegistry.Lookup(name) ?? Create(true, false, false, name));
            }
        }

        return _superTypes;
    }

    private IVObject Prototype()
    {
        if (_prototype != null) return _prototype;

        if (_prototypeFactory == null)
        {
            throw new InvalidOperationException(
                $"Cannot reflect on type '{Name}' without an instance: it has no prototype " +
                "factory. Interface-only and non-model types cannot be instantiated.");
        }

        return _prototype = _prototypeFactory();
    }

    public bool Equals(VmfType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsModelType == other.IsModelType
            && IsListType == other.IsListType
            && Name == other.Name;
    }

    public override bool Equals(object? obj) => Equals(obj as VmfType);

    public override int GetHashCode() => HashCode.Combine(IsModelType, IsListType, Name);

    public override string ToString() =>
        $"[ name={Name}, modelType={IsModelType}, listType={IsListType} ]";
}
