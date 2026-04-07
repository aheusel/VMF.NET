// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace VMF.NET.Core;

/// <summary>
/// Roslyn-independent description of a model interface.
/// The source generator extracts this from INamedTypeSymbol.
/// </summary>
public sealed class TypeSymbolData
{
    /// <summary>Simple name (e.g. "IParent").</summary>
    public string Name { get; set; } = "";

    /// <summary>Full name including namespace.</summary>
    public string FullName { get; set; } = "";

    /// <summary>Whether this is an interface.</summary>
    public bool IsInterface { get; set; }

    /// <summary>Whether [Immutable] is present.</summary>
    public bool IsImmutable { get; set; }

    /// <summary>Whether [InterfaceOnly] is present.</summary>
    public bool IsInterfaceOnly { get; set; }

    /// <summary>[ExternalType] namespace, or null if not external.</summary>
    public string? ExternalTypeNamespace { get; set; }

    /// <summary>[VmfModel] attribute data, or null.</summary>
    public VmfModelData? VmfModelAttribute { get; set; }

    /// <summary>[VmfEquals] attribute data, or null.</summary>
    public VmfEqualsData? VmfEqualsAttribute { get; set; }

    /// <summary>[Doc] text, or null.</summary>
    public string? Documentation { get; set; }

    /// <summary>Full names of base interfaces that are model types.</summary>
    public List<string> BaseTypeNames { get; set; } = new();

    /// <summary>Properties declared on this interface.</summary>
    public List<PropertySymbolData> Properties { get; set; } = new();

    /// <summary>Constructor delegation, or null.</summary>
    public DelegationSymbolData? ConstructorDelegation { get; set; }

    /// <summary>Method delegations (non-property methods).</summary>
    public List<DelegationSymbolData> MethodDelegations { get; set; } = new();

    /// <summary>Custom annotations.</summary>
    public List<AnnotationData> Annotations { get; set; } = new();
}

/// <summary>
/// Roslyn-independent description of a property on a model interface.
/// </summary>
public sealed class PropertySymbolData
{
    /// <summary>Property name (PascalCase).</summary>
    public string Name { get; set; } = "";

    /// <summary>Full type name of the property.</summary>
    public string FullTypeName { get; set; } = "";

    /// <summary>Simple type name.</summary>
    public string SimpleTypeName { get; set; } = "";

    /// <summary>Namespace of the property type.</summary>
    public string? TypeNamespace { get; set; }

    /// <summary>Whether this is a value type (int, bool, etc.).</summary>
    public bool IsPrimitive { get; set; }

    /// <summary>Whether this is a collection type (IList&lt;T&gt;, VList&lt;T&gt;).</summary>
    public bool IsCollection { get; set; }

    /// <summary>For collections: element type simple name.</summary>
    public string? CollectionElementSimpleName { get; set; }

    /// <summary>For collections: element type namespace.</summary>
    public string? CollectionElementNamespace { get; set; }

    /// <summary>[Contains] opposite property ref, or null.</summary>
    public string? ContainsOpposite { get; set; }

    /// <summary>[Container] opposite property ref, or null.</summary>
    public string? ContainerOpposite { get; set; }

    /// <summary>[Refers] opposite property ref, or null.</summary>
    public string? RefersOpposite { get; set; }

    /// <summary>Whether [VmfRequired] is present.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Whether [IgnoreEquals] is present.</summary>
    public bool IsIgnoredForEquals { get; set; }

    /// <summary>Whether [IgnoreToString] is present.</summary>
    public bool IsIgnoredForToString { get; set; }

    /// <summary>Whether [GetterOnly] is present.</summary>
    public bool IsGetterOnly { get; set; }

    /// <summary>[VmfDefaultValue] value, or null.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>[PropertyOrder] index, or null.</summary>
    public int? OrderIndex { get; set; }

    /// <summary>[Doc] text, or null.</summary>
    public string? Documentation { get; set; }

    /// <summary>Custom annotations.</summary>
    public List<AnnotationData> Annotations { get; set; } = new();
}

/// <summary>
/// Roslyn-independent description of a method delegation.
/// </summary>
public sealed class DelegationSymbolData
{
    /// <summary>Full type name of the delegation target.</summary>
    public string FullTypeName { get; set; } = "";

    /// <summary>Method name.</summary>
    public string MethodName { get; set; } = "";

    /// <summary>Return type.</summary>
    public string ReturnType { get; set; } = "void";

    /// <summary>Parameter types.</summary>
    public List<string> ParamTypes { get; set; } = new();

    /// <summary>Parameter names.</summary>
    public List<string> ParamNames { get; set; } = new();

    /// <summary>Custom documentation.</summary>
    public string? Documentation { get; set; }
}

/// <summary>[VmfModel] attribute data.</summary>
public sealed class VmfModelData
{
    public EqualsStrategy Value { get; set; } = EqualsStrategy.Instance;
}

/// <summary>[VmfEquals] attribute data.</summary>
public sealed class VmfEqualsData
{
    public EqualsStrategy Value { get; set; } = EqualsStrategy.ContainmentAndExternal;
}

/// <summary>Custom annotation key-value pair.</summary>
public sealed class AnnotationData
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
