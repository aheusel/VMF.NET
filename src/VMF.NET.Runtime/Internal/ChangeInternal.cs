// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Internal change interface with additional metadata for cross-ref and containment tracking.
/// </summary>
internal interface IChangeInternal : IChange
{
    /// <summary>
    /// Internal event info string for cross-ref/containment change classification.
    /// </summary>
    string InternalChangeInfo { get; }

    static bool IsCrossRefChange(IChange change)
    {
        return change is IChangeInternal ci && ci.InternalChangeInfo == ChangeTypeConstants.CrossRef;
    }

    static bool IsContainmentChange(IChange change)
    {
        return change is IChangeInternal ci && ci.InternalChangeInfo == ChangeTypeConstants.Containment;
    }
}

internal static class ChangeTypeConstants
{
    public const string CrossRef = "vmf:change:type:crossref";
    public const string Containment = "vmf:change:type:containment";
    public const string Empty = "";
}
