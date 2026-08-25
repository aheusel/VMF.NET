// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VMF.NET.Core;

/// <summary>
/// Represents a single type (interface) in a VMF model.
/// Port of Java ModelType.java.
/// </summary>
public sealed class ModelTypeInfo
{
    public ModelTypeInfo(ModelInfo model, string typeName, string namespaceName, int typeId)
    {
        Model = model;
        TypeName = typeName;
        NamespaceName = namespaceName;
        TypeId = typeId;
    }

    /// <summary>The owning model.</summary>
    public ModelInfo Model { get; }

    /// <summary>Simple type name (e.g. "IParent").</summary>
    public string TypeName { get; }

    /// <summary>Namespace (e.g. "MyApp.Models").</summary>
    public string NamespaceName { get; }

    /// <summary>Full type name including namespace.</summary>
    public string FullTypeName => $"{NamespaceName}.{TypeName}";

    /// <summary>
    /// Unique type ID. Incremented by 2 per type to accommodate read-only variant.
    /// </summary>
    public int TypeId { get; }

    /// <summary>Whether this type is marked [Immutable].</summary>
    public bool IsImmutable { get; set; }

    /// <summary>Whether this type is marked [InterfaceOnly].</summary>
    public bool IsInterfaceOnly { get; set; }

    /// <summary>Equals/HashCode strategy for this type.</summary>
    public EqualsStrategy? EqualsStrategy { get; set; }

    /// <summary>Custom documentation text.</summary>
    public string? Documentation { get; set; }

    /// <summary>Whether custom property ordering is present.</summary>
    public bool IsCustomPropertyOrderPresent { get; set; }

    // --- Properties ---

    /// <summary>Properties declared directly on this type's interface.</summary>
    public List<PropertyInfo> Properties { get; } = new();

    /// <summary>All properties including inherited ones (populated in later passes).</summary>
    public List<PropertyInfo> AllProperties { get; } = new();

    // --- Relationships ---

    /// <summary>Types directly extended by this type.</summary>
    public List<ModelTypeInfo> Implements { get; } = new();

    /// <summary>All inherited types (transitive closure).</summary>
    public List<ModelTypeInfo> AllInheritedTypes { get; } = new();

    // --- Delegations ---

    /// <summary>All delegations (method + constructor).</summary>
    public List<DelegationInfo> Delegations { get; } = new();

    /// <summary>Method delegations only. Includes inherited ones after analysis.</summary>
    public List<DelegationInfo> MethodDelegations { get; } = new();

    /// <summary>
    /// Method delegations DECLARED on this type, before inheritance is applied. The generated
    /// interface declares these; the inherited ones already appear on the base interface.
    /// </summary>
    public List<DelegationInfo> OwnMethodDelegations { get; } = new();

    /// <summary>Constructor delegations only.</summary>
    public List<DelegationInfo> ConstructorDelegations { get; } = new();

    /// <summary>
    /// One delegation per delegate class — the fields the implementation declares. An object holds
    /// a single instance of each delegate class and shares it between the constructor hook and
    /// every delegated method, as Java does.
    /// </summary>
    public List<DelegationInfo> DelegationsOneForEachType =>
        Delegations
            .Where(d => !d.IsExclusivelyForInterfaceOnlyTypes)
            .GroupBy(d => d.FullTypeName)
            .Select(g => g.First())
            .ToList();

    // --- Annotations ---

    /// <summary>Custom annotations on this type.</summary>
    public List<AnnotationInfo> Annotations { get; } = new();

    // --- Computed properties ---

    /// <summary>Effective equals strategy (falls back to model default).</summary>
    public EqualsStrategy EffectiveEqualsStrategy =>
        EqualsStrategy ?? Model.Config.EqualsDefault;

    /// <summary>Whether content-based equals is enabled.</summary>
    public bool IsEqualsAndHashCode =>
        EffectiveEqualsStrategy != Core.EqualsStrategy.Instance;

    /// <summary>Whether ALL equals strategy is used.</summary>
    public bool IsEqualsAll =>
        EffectiveEqualsStrategy == Core.EqualsStrategy.All;

    /// <summary>Whether CONTAINMENT_AND_EXTERNAL strategy is used.</summary>
    public bool IsEqualsContainmentAndExternal =>
        EffectiveEqualsStrategy == Core.EqualsStrategy.ContainmentAndExternal;

    /// <summary>Whether this type has documentation.</summary>
    public bool IsDocumented => !string.IsNullOrEmpty(Documentation);

    /// <summary>
    /// Whether this is an interface-only type with only getter-only properties.
    /// </summary>
    public bool IsInterfaceOnlyWithGettersOnly =>
        IsInterfaceOnly && AllProperties.All(p => p.IsGetterOnly);

    /// <summary>Resolve a property by name.</summary>
    public PropertyInfo? ResolveProp(string name) =>
        Properties.FirstOrDefault(p => p.Name == name);

    /// <summary>Whether this type extends the specified type (directly or transitively).</summary>
    public bool ExtendsType(ModelTypeInfo type)
    {
        if (FullTypeName == type.FullTypeName) return true;
        return Implements.Any(t => t.FullTypeName == type.FullTypeName)
            || AllInheritedTypes.Any(t => t.FullTypeName == type.FullTypeName);
    }

    /// <summary>Whether equals() is delegated.</summary>
    public bool IsEqualsMethodDelegated =>
        AllDelegations.Any(d =>
            d.MethodName == "Equals" && d.ParamTypes.Count == 1
            && (d.ParamTypes[0] == "object" || d.ParamTypes[0] == "object?")
            && d.ReturnType == "bool");

    /// <summary>Whether GetHashCode() is delegated.</summary>
    public bool IsHashCodeMethodDelegated =>
        AllDelegations.Any(d =>
            d.MethodName == "GetHashCode" && d.ParamTypes.Count == 0
            && d.ReturnType == "int");

    /// <summary>Whether ToString() is delegated.</summary>
    public bool IsToStringMethodDelegated =>
        AllDelegations.Any(d =>
            d.MethodName == "ToString" && d.ParamTypes.Count == 0
            && d.ReturnType == "string");

    /// <summary>All delegations including inherited.</summary>
    private IEnumerable<DelegationInfo> AllDelegations => Delegations;

    /// <summary>
    /// The type name without its interface prefix (e.g. "Parent" for "IParent"). This is the
    /// name Java's model uses throughout, so it is also what derived names are built from.
    /// </summary>
    public string SimpleName => StripInterfacePrefix(TypeName);

    /// <summary>
    /// The implementation class name (e.g. "ParentImpl").
    /// Strips leading "I" from interface name.
    /// </summary>
    public string ImplClassName => SimpleName + "Impl";

    /// <summary>
    /// The read-only interface name (e.g. "IReadOnlyParent").
    /// </summary>
    public string ReadOnlyInterfaceName => "IReadOnly" + SimpleName;

    /// <summary>
    /// The read-only implementation class name (e.g. "ReadOnlyParentImpl").
    /// </summary>
    public string ReadOnlyImplClassName => "ReadOnly" + SimpleName + "Impl";

    /// <summary>
    /// Drops a leading "I" that is followed by another capital, so "IParent" becomes "Parent"
    /// but "Item" is left alone.
    /// </summary>
    public static string StripInterfacePrefix(string typeName) =>
        typeName.StartsWith("I") && typeName.Length > 1 && char.IsUpper(typeName[1])
            ? typeName.Substring(1)
            : typeName;

    /// <summary>
    /// The last namespace segment that marks a model: <c>MyApp.VmfModel</c> holds the model for
    /// <c>MyApp</c>. Mirrors Java's <c>…vmfmodel</c> package, and being there is what declares an
    /// interface to be a model type — no attribute does.
    /// </summary>
    public const string ModelNamespaceSegment = "VmfModel";

    /// <summary>
    /// The public interface name generated for a model type: <c>"I"</c> + the model's name, unless
    /// the model already begins with <c>I</c> followed by a capital, in which case it is used
    /// unchanged. The inverse of <see cref="StripInterfacePrefix"/>.
    /// <para>
    /// The second case is what keeps a model written the old way working: a model named
    /// <c>IParent</c> still yields <c>IParent</c>, so only its namespace has to move.
    /// </para>
    /// </summary>
    public static string ApiInterfaceName(string modelTypeName) =>
        modelTypeName.StartsWith("I") && modelTypeName.Length > 1 && char.IsUpper(modelTypeName[1])
            ? modelTypeName
            : "I" + modelTypeName;

    public override string ToString() => FullTypeName;
}
