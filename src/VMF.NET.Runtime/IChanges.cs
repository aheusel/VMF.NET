// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// Change tracking API for recording, listening to, and undoing model changes.
/// Accessed via <c>obj.Vmf().Changes()</c>.
/// </summary>
public interface IChanges
{
    /// <summary>
    /// Adds a change listener that is notified about changes to all objects in the current object graph.
    /// Returns an <see cref="IDisposable"/> that unsubscribes the listener when disposed.
    /// </summary>
    IDisposable AddListener(Action<IChange> listener);

    /// <summary>
    /// Adds a change listener. If <paramref name="recursive"/> is true, registers with all
    /// objects in the current object graph.
    /// </summary>
    IDisposable AddListener(Action<IChange> listener, bool recursive);

    /// <summary>
    /// Starts recording changes. Previously recorded changes are removed.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops recording changes. Unpublished transactions are published.
    /// </summary>
    void Stop();

    /// <summary>
    /// Starts a new transaction.
    /// </summary>
    void StartTransaction();

    /// <summary>
    /// Publishes a transaction consisting of all changes since the last
    /// <see cref="StartTransaction"/> or <see cref="PublishTransaction"/> call.
    /// </summary>
    void PublishTransaction();

    /// <summary>
    /// Returns all recorded changes since the last <see cref="Start"/> call.
    /// </summary>
    VList<IChange> All();

    /// <summary>
    /// Returns all published transactions since the last <see cref="Start"/> call.
    /// </summary>
    VList<ITransaction> Transactions();

    /// <summary>
    /// Removes all recorded changes and transactions.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns the current model version.
    /// </summary>
    IModelVersion ModelVersion();

    /// <summary>
    /// Indicates whether model versioning is enabled.
    /// </summary>
    bool IsModelVersioningEnabled { get; }
}
