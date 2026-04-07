// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VMF.NET.Core;

/// <summary>
/// Analyzes VMF model interfaces and builds a <see cref="ModelInfo"/> graph.
/// Port of Java Model.java's 7-pass analysis, but operating on
/// <see cref="SymbolData"/> records instead of Java reflection.
///
/// This class is Roslyn-independent — the source generator extracts
/// <see cref="SymbolData"/> from <c>INamedTypeSymbol</c> and passes it here.
/// </summary>
public static class ModelAnalyzer
{
    /// <summary>
    /// Builds a <see cref="ModelInfo"/> from a set of interface descriptions.
    /// </summary>
    public static ModelInfo Analyze(string namespaceName, IReadOnlyList<TypeSymbolData> interfaces)
    {
        var model = new ModelInfo(namespaceName);

        if (interfaces.Count == 0)
        {
            model.AddError("At least one interface is required.");
            return model;
        }

        // --- Find model config ---
        foreach (var iface in interfaces)
        {
            if (iface.VmfModelAttribute != null)
            {
                model.Config = new ModelConfig { EqualsDefault = iface.VmfModelAttribute.Value };
            }
        }

        // --- PASS 0.0: Separate external types ---
        var modelInterfaces = new List<TypeSymbolData>();
        foreach (var iface in interfaces)
        {
            if (iface.ExternalTypeNamespace != null)
            {
                model.AddExternalType(iface.Name, iface.ExternalTypeNamespace);
            }
            else
            {
                modelInterfaces.Add(iface);
            }
        }

        // --- PASS 0.1a: Create ModelTypeInfo for each interface (types only) ---
        int typeId = 0;
        var symbolMap = new Dictionary<string, TypeSymbolData>();
        foreach (var iface in modelInterfaces)
        {
            if (!iface.IsInterface)
            {
                model.AddError($"Model may only contain interfaces, but found '{iface.Name}'.");
                continue;
            }

            var typeInfo = model.AddType(iface.Name, typeId);
            typeInfo.IsImmutable = iface.IsImmutable;
            typeInfo.IsInterfaceOnly = iface.IsInterfaceOnly;
            typeInfo.Documentation = iface.Documentation;

            if (iface.VmfEqualsAttribute != null)
            {
                typeInfo.EqualsStrategy = iface.VmfEqualsAttribute.Value;
            }

            // Parse annotations
            foreach (var ann in iface.Annotations)
            {
                typeInfo.Annotations.Add(new AnnotationInfo(ann.Key, ann.Value));
            }
            typeInfo.Annotations.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

            symbolMap[iface.FullName] = iface;
            typeId += 2;
        }

        if (model.HasErrors) return model;

        // --- PASS 0.1b: Initialize properties and delegations (all types exist now) ---
        foreach (var type in model.Types)
        {
            var iface = symbolMap[type.FullTypeName];
            InitProperties(model, type, iface);
            InitDelegations(model, type, iface);
        }

        if (model.HasErrors) return model;

        // --- PASS 1: Resolve containment relationships ---
        foreach (var type in model.Types)
        {
            foreach (var prop in type.Properties)
            {
                var sym = symbolMap[type.FullTypeName];
                var propSym = sym.Properties.FirstOrDefault(p => p.Name == prop.Name);
                if (propSym == null) continue;
                InitContainment(model, type, prop, propSym);
            }
        }

        // --- PASS 2: Resolve cross-references ---
        foreach (var type in model.Types)
        {
            foreach (var prop in type.Properties)
            {
                var sym = symbolMap[type.FullTypeName];
                var propSym = sym.Properties.FirstOrDefault(p => p.Name == prop.Name);
                if (propSym == null) continue;
                InitCrossRef(model, type, prop, propSym);
            }
        }

        // --- PASS 3: Resolve implements (inheritance) ---
        foreach (var type in model.Types)
        {
            var sym = symbolMap[type.FullTypeName];
            foreach (var baseName in sym.BaseTypeNames)
            {
                var resolved = model.ResolveType(baseName);
                if (resolved != null)
                {
                    type.Implements.Add(resolved);
                }
                // Non-model base types are external — skip silently
            }
        }

        // --- PASS 5: Collect all properties (including inherited) ---
        foreach (var type in model.Types)
        {
            CollectAllProperties(type);
        }

        // --- PASS 4: Assign property IDs ---
        foreach (var type in model.Types)
        {
            for (int i = 0; i < type.AllProperties.Count; i++)
            {
                type.AllProperties[i].PropId = i;
            }
        }

        // --- PASS 6: Compute all inherited types ---
        foreach (var type in model.Types)
        {
            ComputeAllInheritedTypes(type, type.AllInheritedTypes, new HashSet<string>());
        }

        // --- PASS 7: Validation ---
        Validate(model);

        return model;
    }

    private static void InitProperties(ModelInfo model, ModelTypeInfo typeInfo, TypeSymbolData symbol)
    {
        bool hasCustomOrder = false;
        bool hasMissingOrder = false;

        foreach (var propSym in symbol.Properties)
        {
            var prop = new PropertyInfo(typeInfo, propSym.Name);
            prop.TypeName = propSym.FullTypeName;
            prop.SimpleTypeName = propSym.SimpleTypeName;
            prop.PackageName = propSym.TypeNamespace ?? "";
            prop.IsRequired = propSym.IsRequired;
            prop.IsIgnoredForEquals = propSym.IsIgnoredForEquals;
            prop.IsIgnoredForToString = propSym.IsIgnoredForToString;
            prop.IsGetterOnly = propSym.IsGetterOnly;
            prop.DefaultValueAsString = propSym.DefaultValue;
            prop.CustomOrderIndex = propSym.OrderIndex;
            prop.Documentation = propSym.Documentation;

            // Classify property type
            if (propSym.IsPrimitive)
            {
                prop.PropType = PropType.Primitive;
            }
            else if (propSym.IsCollection)
            {
                prop.PropType = PropType.Collection;
                prop.GenericTypeName = propSym.CollectionElementSimpleName;
                prop.GenericPackageName = propSym.CollectionElementNamespace;
            }
            else
            {
                prop.PropType = PropType.Class;
            }

            // Resolve model type
            prop.ModelType = model.ResolveType(prop.TypeName);
            if (prop.IsCollectionType && prop.GenericTypeName != null)
            {
                var elementFullName = string.IsNullOrEmpty(prop.GenericPackageName)
                    ? prop.GenericTypeName
                    : $"{prop.GenericPackageName}.{prop.GenericTypeName}";
                prop.GenericModelType = model.ResolveType(elementFullName);
            }

            // Parse annotations
            foreach (var ann in propSym.Annotations)
            {
                prop.Annotations.Add(new AnnotationInfo(ann.Key, ann.Value));
            }
            prop.Annotations.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

            if (prop.CustomOrderIndex != null) hasCustomOrder = true;
            else hasMissingOrder = true;

            typeInfo.Properties.Add(prop);
        }

        if (hasCustomOrder && hasMissingOrder)
        {
            model.AddError($"Type '{typeInfo.TypeName}' has incomplete property order (annotate all or none).");
        }

        if (hasCustomOrder && !hasMissingOrder)
        {
            typeInfo.IsCustomPropertyOrderPresent = true;
            // Check for duplicate indices
            var dupes = typeInfo.Properties
                .Where(p => p.CustomOrderIndex.HasValue)
                .GroupBy(p => p.CustomOrderIndex!.Value)
                .Where(g => g.Count() > 1)
                .ToList();
            if (dupes.Count > 0)
            {
                model.AddError($"Type '{typeInfo.TypeName}' has duplicate property order indices.");
            }
        }

        SortProperties(typeInfo.Properties, typeInfo.IsCustomPropertyOrderPresent);
    }

    private static void SortProperties(List<PropertyInfo> properties, bool customOrder)
    {
        if (customOrder)
            properties.Sort((a, b) => (a.CustomOrderIndex ?? 0).CompareTo(b.CustomOrderIndex ?? 0));
        else
            properties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    private static void InitDelegations(ModelInfo model, ModelTypeInfo typeInfo, TypeSymbolData symbol)
    {
        // Constructor delegation
        if (symbol.ConstructorDelegation != null)
        {
            var cd = symbol.ConstructorDelegation;
            var info = new DelegationInfo(cd.FullTypeName, "", "void", new(), new(), true, cd.Documentation);
            typeInfo.Delegations.Add(info);
            typeInfo.ConstructorDelegations.Add(info);
        }

        // Method delegations
        foreach (var del in symbol.MethodDelegations)
        {
            var info = new DelegationInfo(
                del.FullTypeName, del.MethodName, del.ReturnType,
                new List<string>(del.ParamTypes), new List<string>(del.ParamNames),
                false, del.Documentation);

            if ((!string.IsNullOrEmpty(info.FullTypeName)) ||
                (info.IsExclusivelyForInterfaceOnlyTypes && typeInfo.IsInterfaceOnly))
            {
                typeInfo.Delegations.Add(info);
                typeInfo.MethodDelegations.Add(info);
            }
            else
            {
                model.AddError(
                    $"Custom method '{typeInfo.TypeName}.{del.MethodName}(...)' does not define a delegation class.");
            }
        }
    }

    private static void InitContainment(
        ModelInfo model, ModelTypeInfo type, PropertyInfo prop, PropertySymbolData sym)
    {
        if (sym.ContainsOpposite != null)
        {
            // This property contains (owns) objects
            if (sym.ContainsOpposite == "")
            {
                // No opposite specified — allowed for @Contains
                prop.Containment = new ContainmentInfo(type, prop, null, null, ContainmentType.Contained);
            }
            else
            {
                var opposite = ResolveOppositeFromProperty(model, type, prop, sym.ContainsOpposite);
                if (opposite != null)
                {
                    prop.Containment = new ContainmentInfo(type, prop, opposite.Parent, opposite, ContainmentType.Contained);
                }
                else
                {
                    model.AddError($"Cannot resolve @Contains opposite '{sym.ContainsOpposite}' for '{type.TypeName}.{prop.Name}'.");
                }
            }
        }
        else if (sym.ContainerOpposite != null)
        {
            // This property references the container (parent)
            if (sym.ContainerOpposite == "")
            {
                prop.IsReadOnly = true;
                prop.Containment = new ContainmentInfo(null, null, null, null, ContainmentType.Container);
            }
            else
            {
                var opposite = ResolveOppositeFromProperty(model, type, prop, sym.ContainerOpposite);
                if (opposite != null)
                {
                    prop.Containment = new ContainmentInfo(type, prop, opposite.Parent, opposite, ContainmentType.Container);
                }
                else
                {
                    model.AddError($"Cannot resolve @Container opposite '{sym.ContainerOpposite}' for '{type.TypeName}.{prop.Name}'.");
                }
            }
        }
    }

    private static void InitCrossRef(
        ModelInfo model, ModelTypeInfo type, PropertyInfo prop, PropertySymbolData sym)
    {
        if (sym.RefersOpposite == null) return;

        var opposite = ResolveOppositeFromProperty(model, type, prop, sym.RefersOpposite);
        if (opposite != null)
        {
            prop.Reference = new ReferenceInfo(type, prop, opposite.Parent, opposite);
        }
        else
        {
            model.AddError($"Cannot resolve @Refers opposite '{sym.RefersOpposite}' for '{type.TypeName}.{prop.Name}'.");
        }
    }

    private static PropertyInfo? ResolveOppositeFromProperty(
        ModelInfo model, ModelTypeInfo ownerType, PropertyInfo prop, string oppositeRef)
    {
        // Try "TypeName.PropName" format first
        var result = model.ResolveOpposite(ownerType, oppositeRef);
        if (result != null) return result;

        // Try with the property's type as prefix: "PropTypeName.oppositePropName"
        string propTypeName = prop.IsCollectionType
            ? (prop.GenericTypeName ?? "")
            : prop.SimpleTypeName;

        if (!string.IsNullOrEmpty(propTypeName) && !oppositeRef.Contains('.'))
        {
            result = model.ResolveOpposite(ownerType, $"{propTypeName}.{oppositeRef}");
        }

        return result;
    }

    private static void CollectAllProperties(ModelTypeInfo type)
    {
        // Start with own properties
        var seen = new HashSet<string>();
        type.AllProperties.Clear();

        // Add inherited properties first
        foreach (var baseType in type.Implements)
        {
            foreach (var baseProp in baseType.Properties)
            {
                if (seen.Add(baseProp.Name))
                {
                    // Check if this type overrides the property
                    var ownProp = type.Properties.FirstOrDefault(p => p.Name == baseProp.Name);
                    type.AllProperties.Add(ownProp ?? baseProp);
                    if (ownProp != null) seen.Add(ownProp.Name);
                }
            }
        }

        // Add own properties not yet added
        foreach (var prop in type.Properties)
        {
            if (seen.Add(prop.Name))
            {
                type.AllProperties.Add(prop);
            }
        }
    }

    private static void ComputeAllInheritedTypes(
        ModelTypeInfo type, List<ModelTypeInfo> result, HashSet<string> visited)
    {
        foreach (var baseType in type.Implements)
        {
            if (visited.Add(baseType.FullTypeName))
            {
                result.Add(baseType);
                ComputeAllInheritedTypes(baseType, result, visited);
            }
        }
    }

    private static void Validate(ModelInfo model)
    {
        foreach (var type in model.Types)
        {
            // Equals/hashCode delegation consistency
            if (type.IsEqualsMethodDelegated != type.IsHashCodeMethodDelegated)
            {
                model.AddError(type.IsHashCodeMethodDelegated
                    ? "If GetHashCode() is delegated, Equals(object) must be too."
                    : "If Equals(object) is delegated, GetHashCode() must be too.",
                    type.FullTypeName);
            }

            if (!type.IsImmutable)
            {
                // Mutable types cannot extend immutable types
                foreach (var iType in type.AllInheritedTypes)
                {
                    if (iType.IsImmutable)
                    {
                        model.AddError(
                            $"Mutable type '{type.FullTypeName}' cannot extend immutable type '{iType.FullTypeName}'.",
                            type.FullTypeName);
                    }
                }

                // Mutable types cannot have getter-only properties (unless interface-only)
                foreach (var p in type.Properties)
                {
                    if (!type.IsInterfaceOnly && p.IsGetterOnly)
                    {
                        model.AddError(
                            $"Mutable type '{type.FullTypeName}' cannot have getter-only property '{p.Name}'.",
                            type.FullTypeName);
                    }

                    // Immutable types cannot be contained
                    if (p.ModelType is { IsImmutable: true })
                    {
                        if (p.IsContainer)
                            model.AddError($"Immutable type cannot be contained: '{type.FullTypeName}.{p.Name}'.");
                        if (p.IsContained)
                            model.AddError($"Immutable type cannot be container: '{type.FullTypeName}.{p.Name}'.");
                    }
                }
            }
            else
            {
                // Immutable types can only extend immutable or interface-only-with-getters types
                foreach (var iType in type.AllInheritedTypes)
                {
                    if (!iType.IsImmutable && !iType.IsInterfaceOnlyWithGettersOnly)
                    {
                        model.AddError(
                            $"Immutable type '{type.FullTypeName}' cannot extend mutable type '{iType.FullTypeName}'.",
                            type.FullTypeName);
                    }
                }

                // Immutable types cannot have mutable model-type properties
                foreach (var p in type.AllProperties)
                {
                    if (p.ModelType is { IsImmutable: false })
                    {
                        model.AddError(
                            $"Immutable type '{type.FullTypeName}' cannot have mutable property '{p.Name}'.");
                    }

                    if (p.IsCollectionType && p.GenericModelType is { IsImmutable: false })
                    {
                        model.AddError(
                            $"Immutable type '{type.FullTypeName}' cannot have collection with mutable element type '{p.GenericModelType.FullTypeName}'.");
                    }

                    if (p.IsContainer || p.IsContained)
                    {
                        model.AddError(
                            $"Immutable type '{type.FullTypeName}' cannot participate in containment (property '{p.Name}').");
                    }
                }
            }
        }
    }
}
