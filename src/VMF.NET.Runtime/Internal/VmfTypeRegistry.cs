// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Runtime.Internal;

/// <summary>
/// Maps a model type's full name to the type object for it.
/// <para>
/// Java resolves a type name to its class with <c>Class.forName</c> when it needs the super types
/// of a property's type. There is no equivalent here that does not involve runtime reflection, so
/// each generated model registers its types instead: one registration per model type, run when
/// the assembly loads.
/// </para>
/// </summary>
public static class VmfTypeRegistry
{
    private static readonly Dictionary<string, VmfType> _types = [];
    private static readonly object _lock = new();

    /// <summary>
    /// Registers a model type. Called by generated code; registering the same name twice keeps
    /// the first registration.
    /// </summary>
    public static void Register(string name, VmfType type)
    {
        lock (_lock)
        {
            if (!_types.ContainsKey(name)) _types[name] = type;
        }
    }

    /// <summary>Returns the registered type for a full type name, or null if unknown.</summary>
    public static VmfType? Lookup(string name)
    {
        lock (_lock)
        {
            return _types.TryGetValue(name, out var t) ? t : null;
        }
    }

    /// <summary>Every registered model type, in registration order per model.</summary>
    public static IReadOnlyList<VmfType> All()
    {
        lock (_lock)
        {
            return _types.Values.ToList();
        }
    }

    /// <summary>Every registered model type whose name starts with the given namespace prefix.</summary>
    public static IReadOnlyList<VmfType> AllInNamespace(string namespacePrefix)
    {
        string prefix = namespacePrefix.EndsWith(".") ? namespacePrefix : namespacePrefix + ".";
        lock (_lock)
        {
            return _types.Where(kv => kv.Key.StartsWith(prefix, StringComparison.Ordinal))
                         .Select(kv => kv.Value)
                         .ToList();
        }
    }
}
