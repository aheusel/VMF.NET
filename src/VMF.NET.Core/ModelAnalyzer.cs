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
            // [VmfModel] is no longer a marker -- the namespace decides that (C-6). It survives
            // only as model-wide configuration, so an interface declares the default for its
            // whole namespace by setting Equality explicitly.
            if (iface.VmfModelAttribute?.Value is { } equalsDefault)
            {
                model.Config = new ModelConfig { EqualsDefault = equalsDefault };
            }
        }

        // [ExternalType] stand-ins never reach here: they are not model types, so discovery
        // skips them and SymbolExtractor resolves references to them straight to the type they
        // name. Nothing to separate.
        var modelInterfaces = interfaces;

        // --- PASS 0.1a: Create ModelTypeInfo for each interface (types only) ---
        int typeId = 0;
        var symbolMap = new Dictionary<string, TypeSymbolData>();

        // The generated interface keeps the model's name verbatim, so `Horse` and `IHorse` no
        // longer collide there. Their IMPLEMENTATIONS still do: both are `HorseImpl`, because the
        // impl name is the model name with any leading `I` stripped. Keyed on that.
        var implClaimedBy = new Dictionary<string, string>();

        foreach (var iface in modelInterfaces)
        {
            if (!iface.IsInterface)
            {
                model.AddError($"Model may only contain interfaces, but found '{iface.Name}'.");
                continue;
            }

            var implName = ModelTypeInfo.StripInterfacePrefix(iface.Name) + "Impl";

            if (implClaimedBy.TryGetValue(implName, out var firstModelName))
            {
                model.AddError(
                    $"Model interfaces '{firstModelName}' and '{iface.ModelName}' would both be "
                    + $"implemented by '{namespaceName}.{implName}'. The implementation name is the "
                    + "model name with any leading 'I' stripped, so these two collide even though "
                    + "their interfaces do not. Rename one of them.");
                continue;
            }
            implClaimedBy[implName] = iface.ModelName;

            // The interface keeps the model's name, but the implementation cannot: `IHorseImpl`
            // would be a class named like an interface. So a leading `I` is dropped, and that is
            // the one place a name still changes without being asked for -- report it.
            //
            // VMF004, so it can be silenced on its own:  <NoWarn>VMF004</NoWarn>
            if (ModelTypeInfo.HasInterfacePrefix(iface.Name))
            {
                model.AddWarning(
                    $"Model interface '{iface.Name}' is implemented by '{implName}': the leading 'I' "
                    + "is stripped so the implementation is not named like an interface. Name the "
                    + $"model '{ModelTypeInfo.StripInterfacePrefix(iface.Name)}' to avoid the "
                    + "asymmetry, or silence this with <NoWarn>"
                    + Diagnostic.PrefixStrippedId + "</NoWarn>.",
                    id: Diagnostic.PrefixStrippedId);
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

        // --- PASS 6b: Inherit delegations from supertypes ---
        // Snapshot first: every type reads its supertypes' DECLARED delegations, so nothing may
        // observe a list that an earlier iteration has already extended.
        var declaredDelegations = model.Types.ToDictionary(
            t => t.FullTypeName,
            t => new List<DelegationInfo>(t.Delegations));
        foreach (var type in model.Types)
        {
            InheritDelegations(type, declaredDelegations);
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
            prop.IsNullableValueType = propSym.IsNullableValueType;
            prop.IsRequired = propSym.IsRequired;
            prop.IsIgnoredForEquals = propSym.IsIgnoredForEquals;
            prop.IsIgnoredForToString = propSym.IsIgnoredForToString;
            prop.IsGetterOnly = propSym.IsGetterOnly;
            prop.DefaultValueAsString = propSym.DefaultValue;
            prop.CustomOrderIndex = propSym.OrderIndex;
            prop.Documentation = propSym.Documentation;

            // A model must declare a collection as an array, so that it never names the
            // collection type and the generated API is free to change it. Reported rather than
            // tolerated: the property would otherwise be classified as a plain reference below,
            // silently generating the wrong thing for a model written against the old rule.
            if (propSym.LegacyCollectionSpelling is { } legacySpelling)
            {
                model.AddError(
                    $"Property '{typeInfo.TypeName}.{prop.Name}' declares its collection as "
                    + $"'{legacySpelling}<{propSym.CollectionElementSimpleName ?? "T"}>'. Declare it as "
                    + $"'{propSym.CollectionElementSimpleName ?? "T"}[]' instead — VMF.NET follows Java VMF, "
                    + "where a model writes an array and the generator produces the VList property.");
            }

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
            var info = new DelegationInfo(
                cd.FullTypeName, cd.MethodName, "void", new(), new(), true, cd.Documentation,
                cd.CallerTypeName);
            typeInfo.Delegations.Add(info);
            typeInfo.ConstructorDelegations.Add(info);
        }

        // Method delegations
        foreach (var del in symbol.MethodDelegations)
        {
            var info = new DelegationInfo(
                del.FullTypeName, del.MethodName, del.ReturnType,
                new List<string>(del.ParamTypes), new List<string>(del.ParamNames),
                false, del.Documentation, del.CallerTypeName);

            if ((!string.IsNullOrEmpty(info.FullTypeName)) ||
                (info.IsExclusivelyForInterfaceOnlyTypes && typeInfo.IsInterfaceOnly))
            {
                typeInfo.Delegations.Add(info);
                typeInfo.MethodDelegations.Add(info);
                typeInfo.OwnMethodDelegations.Add(info);
            }
            else
            {
                model.AddError(
                    $"Custom method '{typeInfo.TypeName}.{del.MethodName}(...)' does not define a delegation class.");
            }
        }
    }

    /// <summary>
    /// Adds the delegations a type inherits from its supertypes, after its own, and reduces the
    /// result to one entry per signature so the nearest declaration wins. Mirrors Java's
    /// <c>Implementation.initPropertiesImportsAndDelegates</c>: <c>ModelType</c> holds only what a
    /// type declares, and inheritance is applied when the implementation is built.
    /// </summary>
    private static void InheritDelegations(
        ModelTypeInfo type, Dictionary<string, List<DelegationInfo>> declared)
    {
        var inherited = new List<DelegationInfo>();
        CollectInheritedDelegations(type, declared, inherited);
        if (inherited.Count == 0) return;

        // Own first, so a redeclaration in this type overrides the inherited one. Every
        // constructor delegation shares the signature "constructor-()", which is what leaves
        // exactly one of those per implementation.
        var all = new List<DelegationInfo>(type.Delegations);
        all.AddRange(inherited);

        var seen = new HashSet<string>();
        type.Delegations.Clear();
        type.MethodDelegations.Clear();
        type.ConstructorDelegations.Clear();

        foreach (var del in all)
        {
            if (!seen.Add(del.MethodSignature)) continue;

            type.Delegations.Add(del);
            if (del.IsConstructorDelegation) type.ConstructorDelegations.Add(del);
            else type.MethodDelegations.Add(del);
        }
    }

    private static void CollectInheritedDelegations(
        ModelTypeInfo type, Dictionary<string, List<DelegationInfo>> declared, List<DelegationInfo> result)
    {
        foreach (var baseType in type.Implements)
        {
            if (declared.TryGetValue(baseType.FullTypeName, out var baseDelegations))
            {
                result.AddRange(baseDelegations);
            }
            CollectInheritedDelegations(baseType, declared, result);
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
        var seen = new HashSet<string>();
        type.AllProperties.Clear();

        // Add inherited properties first, walking the FULL base hierarchy (not just the
        // direct bases): with diamond inheritance (C : A, B where A and B both extend Root)
        // Root's properties are only reachable transitively, and missing them makes the
        // generated implementation fail to implement the interface.
        CollectInheritedProperties(type, type, seen, new HashSet<string>());

        // Add own properties not yet added
        foreach (var prop in type.Properties)
        {
            if (seen.Add(prop.Name))
            {
                type.AllProperties.Add(prop);
            }
        }
    }

    private static void CollectInheritedProperties(
        ModelTypeInfo target, ModelTypeInfo current,
        HashSet<string> seenProps, HashSet<string> visitedTypes)
    {
        foreach (var baseType in current.Implements)
        {
            // guard against diamonds and cycles
            if (!visitedTypes.Add(baseType.FullTypeName)) continue;

            // base-most properties first, so ordering stays stable for linear hierarchies
            CollectInheritedProperties(target, baseType, seenProps, visitedTypes);

            foreach (var baseProp in baseType.Properties)
            {
                if (seenProps.Contains(baseProp.Name)) continue;

                // A property RE-DECLARED on the deriving type is ordered with that type's own
                // properties, not left at its position in the base: re-declaring is how a
                // subtype restates [PropertyOrder], and the restated order must win.
                if (target.Properties.Any(p => p.Name == baseProp.Name)) continue;

                if (seenProps.Add(baseProp.Name))
                {
                    var ownProp = target.Properties.FirstOrDefault(p => p.Name == baseProp.Name);

                    // PropId is assigned per type by index into AllProperties. Inherited
                    // properties must therefore NOT be shared instances -- a property that
                    // lands at a different index in two types would otherwise have its PropId
                    // overwritten, producing duplicate switch cases in the generated code.
                    target.AllProperties.Add(ownProp ?? CopyInherited(baseProp));
                }
            }
        }
    }

    /// <summary>
    /// Creates a per-type copy of an inherited property. <see cref="PropertyInfo.Parent"/> is
    /// deliberately preserved as the DECLARING type, so generated explicit interface
    /// implementations still target the interface that declares the member.
    /// </summary>
    private static PropertyInfo CopyInherited(PropertyInfo source)
    {
        var copy = new PropertyInfo(source.Parent, source.Name)
        {
            TypeName = source.TypeName,
            SimpleTypeName = source.SimpleTypeName,
            PackageName = source.PackageName,
            PropType = source.PropType,
            IsNullableValueType = source.IsNullableValueType,
            ModelType = source.ModelType,
            GenericTypeName = source.GenericTypeName,
            GenericPackageName = source.GenericPackageName,
            GenericModelType = source.GenericModelType,
            Containment = source.Containment,
            Reference = source.Reference,
            IsRequired = source.IsRequired,
            IsIgnoredForEquals = source.IsIgnoredForEquals,
            IsIgnoredForToString = source.IsIgnoredForToString,
            IsGetterOnly = source.IsGetterOnly,
            IsReadOnly = source.IsReadOnly,
            DefaultValueAsString = source.DefaultValueAsString,
            CustomOrderIndex = source.CustomOrderIndex,
            Documentation = source.Documentation,
        };
        copy.Annotations.AddRange(source.Annotations);
        return copy;
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

    /// <summary>
    /// A property re-declared at a different type narrows it covariantly. That works for a scalar:
    /// the implementation carries the narrowed member and satisfies each base with a forwarding
    /// explicit implementation. It cannot work for a collection, because <c>VList&lt;T&gt;</c> is
    /// invariant and no forwarding implementation can exist — Java allows it only because its
    /// properties are arrays, which are covariant.
    /// </summary>
    private static void ValidateNarrowedProperties(ModelInfo model, ModelTypeInfo type)
    {
        foreach (var own in type.Properties)
        {
            if (!own.IsCollectionType) continue;

            foreach (var baseType in type.AllInheritedTypes)
            {
                var declared = baseType.Properties.FirstOrDefault(p => p.Name == own.Name);
                if (declared == null || declared.TypeName == own.TypeName) continue;

                model.AddError(
                    $"Property '{type.TypeName}.{own.Name}' re-declares "
                    + $"'{baseType.TypeName}.{own.Name}' with a different collection type "
                    + $"('{declared.TypeName}' -> '{own.TypeName}'). A collection property cannot "
                    + "be narrowed: VList<T> is invariant, so the base declaration cannot be "
                    + "implemented. Declare both at the same element type.",
                    type.FullTypeName);
            }
        }
    }

    private static void Validate(ModelInfo model)
    {
        foreach (var type in model.Types)
        {
            ValidateNarrowedProperties(model, type);

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
