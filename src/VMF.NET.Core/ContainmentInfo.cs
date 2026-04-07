// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Core;

/// <summary>
/// Describes a containment relationship between two properties.
/// </summary>
public sealed class ContainmentInfo
{
    public ContainmentInfo(
        ModelTypeInfo? ownerType,
        PropertyInfo? ownerProp,
        ModelTypeInfo? oppositeType,
        PropertyInfo? oppositeProp,
        ContainmentType containmentType)
    {
        OwnerType = ownerType;
        OwnerProp = ownerProp;
        OppositeType = oppositeType;
        OppositeProp = oppositeProp;
        ContainmentType = containmentType;
    }

    public ModelTypeInfo? OwnerType { get; }
    public PropertyInfo? OwnerProp { get; }
    public ModelTypeInfo? OppositeType { get; }
    public PropertyInfo? OppositeProp { get; }
    public ContainmentType ContainmentType { get; }

    /// <summary>True if this containment has no explicit opposite property.</summary>
    public bool IsWithoutOpposite => OppositeProp == null;

    public static ContainmentInfo None { get; } = new(null, null, null, null, ContainmentType.None);
}
