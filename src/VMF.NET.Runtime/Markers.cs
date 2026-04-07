// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>Marker interface for mutable VMF objects.</summary>
public interface IMutable : IVObject { }

/// <summary>Marker interface for read-only VMF object wrappers.</summary>
public interface IReadOnly : IVObject { }

/// <summary>Marker interface for immutable VMF objects.</summary>
public interface IImmutable : IVObject { }

/// <summary>Marker interface for interface-only VMF types (not instantiable).</summary>
public interface IInterfaceOnly { }
