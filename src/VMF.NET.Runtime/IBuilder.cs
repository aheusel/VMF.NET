// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Marker interface for generated builders.
/// </summary>
public interface IBuilder
{
    /// <summary>
    /// Builds the VMF object.
    /// </summary>
    IVObject Build();
}
