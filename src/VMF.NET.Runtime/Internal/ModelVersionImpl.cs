// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Immutable implementation of <see cref="IModelVersion"/>.
/// </summary>
public sealed class ModelVersionImpl : IModelVersion, IEquatable<ModelVersionImpl>
{
    public ModelVersionImpl(long timestamp, long versionNumber)
    {
        Timestamp = timestamp;
        VersionNumber = versionNumber;
    }

    public long Timestamp { get; }
    public long VersionNumber { get; }

    public bool Equals(ModelVersionImpl? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Timestamp == other.Timestamp && VersionNumber == other.VersionNumber;
    }

    public override bool Equals(object? obj) => Equals(obj as ModelVersionImpl);
    public override int GetHashCode() => HashCode.Combine(Timestamp, VersionNumber);
    public override string ToString() => $"ModelVersion[ts={Timestamp}, v={VersionNumber}]";
}
