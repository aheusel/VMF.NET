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

    /// <summary>
    /// Whether this change is the induced (echo) side of a cross-reference update, and so
    /// should be reported to listeners but not recorded.
    /// </summary>
    static bool IsCrossRefEchoChange(IChange change)
    {
        return change is IChangeInternal ci && ci.InternalChangeInfo == ChangeTypeConstants.CrossRefEcho;
    }
}

internal static class ChangeTypeConstants
{
    public const string CrossRef = "vmf:change:type:crossref";

    /// <summary>
    /// The INDUCED side of a cross-reference update. Setting one side of a cross-reference
    /// also sets the opposite; that second update is an echo of the first, not a change of
    /// its own. It is still reported to listeners, but it is not recorded -- the change
    /// belongs to the object the update was initiated on.
    /// </summary>
    public const string CrossRefEcho = "vmf:change:type:crossref-echo";
    public const string Containment = "vmf:change:type:containment";
    public const string Empty = "";
}
