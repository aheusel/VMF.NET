// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Core;

/// <summary>
/// Custom annotation key-value pair from [VmfAnnotation].
/// </summary>
public sealed class AnnotationInfo : IEquatable<AnnotationInfo>
{
    public AnnotationInfo(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public string Value { get; }

    public bool Equals(AnnotationInfo? other) =>
        other is not null && Key == other.Key;

    public override bool Equals(object? obj) => Equals(obj as AnnotationInfo);
    public override int GetHashCode() => Key.GetHashCode();
    public override string ToString() => $"@{Key}(\"{Value}\")";
}
