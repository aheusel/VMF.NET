// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime;

/// <summary>
/// The type of a model change.
/// </summary>
public enum ChangeType
{
    /// <summary>Change affects a single property.</summary>
    Property,
    /// <summary>Change affects a list.</summary>
    List
}

/// <summary>
/// Represents a single model change (property or list).
/// </summary>
public interface IChange
{
    /// <summary>
    /// The object affected by this change.
    /// </summary>
    IVObject Object { get; }

    /// <summary>
    /// The name of the property affected by this change.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// The type of this change.
    /// </summary>
    ChangeType ChangeType { get; }

    /// <summary>
    /// Timestamp (in ticks) of when this change was created.
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Indicates whether this change can be reverted.
    /// </summary>
    bool IsUndoable { get; }

    /// <summary>
    /// Reverts this change (if possible).
    /// </summary>
    void Undo();

    /// <summary>
    /// Applies this change to the specified target object.
    /// </summary>
    void Apply(IVObject target);

    /// <summary>
    /// The property change details, if this is a property change. Null otherwise.
    /// </summary>
    IPropertyChange? PropertyChange { get; }

    /// <summary>
    /// The list change details, if this is a list change. Null otherwise.
    /// </summary>
    VListChangeEvent? ListChange { get; }
}

/// <summary>
/// Represents a scalar property change with old and new values.
/// </summary>
public interface IPropertyChange
{
    /// <summary>The value prior to this change.</summary>
    object? OldValue { get; }

    /// <summary>The new value after the change.</summary>
    object? NewValue { get; }
}
