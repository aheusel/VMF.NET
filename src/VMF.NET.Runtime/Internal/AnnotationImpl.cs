// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Immutable implementation of <see cref="IAnnotation"/>.
/// </summary>
public sealed class AnnotationImpl : IAnnotation, IEquatable<AnnotationImpl>
{
    public AnnotationImpl(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public string Value { get; }

    public bool Equals(AnnotationImpl? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as AnnotationImpl);
    public override int GetHashCode() => HashCode.Combine(Key, Value);
    public override string ToString() => $"@{Key}(\"{Value}\")";
}
