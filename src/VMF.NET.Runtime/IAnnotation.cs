// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// A runtime annotation containing compile-time metadata queryable via the reflection API.
/// </summary>
public interface IAnnotation
{
    /// <summary>
    /// The annotation key (used to group annotations into categories).
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The annotation value (an arbitrary string; consumers are responsible for parsing).
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Determines whether this annotation matches the specified key and value.
    /// </summary>
    bool Equals(string key, string value) =>
        string.Equals(Key, key) && string.Equals(Value, value);
}
