// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Represents a model version with a timestamp and version number.
/// </summary>
public interface IModelVersion
{
    /// <summary>
    /// The timestamp of this model version (in ticks).
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// The version number. Incremented on each change. Not unique across
    /// different model instances or serialization boundaries.
    /// </summary>
    long VersionNumber { get; }
}
