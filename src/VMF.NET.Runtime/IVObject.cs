// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Base interface for all VMF generated model types.
/// </summary>
public interface IVObject
{
    /// <summary>
    /// Returns the VMF API accessor for this object.
    /// </summary>
    IVmf Vmf();

    /// <summary>
    /// Returns a deep clone of this object.
    /// </summary>
    IVObject Clone();

    /// <summary>
    /// Returns a read-only wrapper of this object.
    /// </summary>
    IVObject AsReadOnly();
}
