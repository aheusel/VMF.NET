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
    /// The VMF API accessor for this object — reflection, change tracking, content operations.
    /// <para>
    /// Java spells this <c>vmf()</c>. It reads state and takes no arguments, so in C# it is a
    /// property. The name does not collide with the <c>VMF</c> namespace: a qualified type name
    /// such as <c>VMF.NET.Runtime.IVmf</c> is resolved in type position, where member lookup is
    /// not consulted.
    /// </para>
    /// </summary>
    IVmf VMF { get; }

    /// <summary>
    /// Returns a deep clone of this object.
    /// </summary>
    IVObject Clone();

    /// <summary>
    /// Returns a read-only wrapper of this object.
    /// </summary>
    IVObject AsReadOnly();
}
