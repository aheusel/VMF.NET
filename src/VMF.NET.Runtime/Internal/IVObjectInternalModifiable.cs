// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Extended internal interface for mutable VMF objects.
/// Generated mutable implementations implement this interface.
/// </summary>
public interface IVObjectInternalModifiable : IVObjectInternal
{
    /// <summary>Sets the property value at the given index.</summary>
    void SetPropertyValueById(int id, object? value);

    /// <summary>Unsets (resets to default) the property at the given index.</summary>
    void UnsetById(int id);

    /// <summary>Sets the default value for the property at the given index.</summary>
    void SetDefaultValueById(int id, object? value);

    /// <summary>Sets the containment parent.</summary>
    void SetContainer(IVObject? container);

    /// <summary>Gets the containment parent.</summary>
    IVObject? GetContainer();

    /// <summary>Gets the property ID of the containment relationship.</summary>
    int GetContainerPropertyId();

    /// <summary>Sets the property ID of the containment relationship.</summary>
    void SetContainerPropertyId(int id);

    /// <summary>Unregisters this object from all containment relationships.</summary>
    void UnregisterFromContainers();

    /// <summary>Registers this model with the change tracking system.</summary>
    void SetModelToChanges(IChanges changes);
}
