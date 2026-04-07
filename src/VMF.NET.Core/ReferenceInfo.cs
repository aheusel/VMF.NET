// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Core;

/// <summary>
/// Describes a bidirectional cross-reference between two properties ([Refers]).
/// </summary>
public sealed class ReferenceInfo
{
    public ReferenceInfo(
        ModelTypeInfo ownerType,
        PropertyInfo ownerProp,
        ModelTypeInfo oppositeType,
        PropertyInfo oppositeProp)
    {
        OwnerType = ownerType;
        OwnerProp = ownerProp;
        OppositeType = oppositeType;
        OppositeProp = oppositeProp;
    }

    public ModelTypeInfo OwnerType { get; }
    public PropertyInfo OwnerProp { get; }
    public ModelTypeInfo OppositeType { get; }
    public PropertyInfo OppositeProp { get; }
}
