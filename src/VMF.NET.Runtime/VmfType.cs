// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Represents a VMF type with metadata about whether it is a model type, list type, etc.
/// Named <c>VmfType</c> to avoid conflict with <see cref="System.Type"/>.
/// </summary>
public sealed class VmfType : IEquatable<VmfType>
{
    private List<VmfType>? _superTypes;

    private VmfType(bool isModelType, bool isListType, bool isInterfaceOnly, string name)
    {
        IsModelType = isModelType;
        IsListType = isListType;
        IsInterfaceOnly = isInterfaceOnly;
        Name = name;
    }

    public static VmfType Create(bool isModelType, bool isListType, bool isInterfaceOnly, string name)
    {
        return new VmfType(isModelType, isListType, isInterfaceOnly, name);
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
    /// Returns the super types of this type (only for model types).
    /// </summary>
    public IReadOnlyList<VmfType> SuperTypes()
    {
        _superTypes ??= [];
        return _superTypes;
    }

    internal void SetSuperTypes(List<VmfType> superTypes)
    {
        _superTypes = superTypes;
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
