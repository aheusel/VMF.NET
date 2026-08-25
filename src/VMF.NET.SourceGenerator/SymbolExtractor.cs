// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.CodeAnalysis;
using VMF.NET.Core;

namespace VMF.NET.SourceGenerator;

/// <summary>
/// Extracts <see cref="TypeSymbolData"/> from Roslyn <see cref="INamedTypeSymbol"/>.
/// This is the bridge between Roslyn's semantic model and the Core analysis layer.
/// </summary>
internal static class SymbolExtractor
{
    /// <summary>
    /// Converts a Roslyn type symbol into a <see cref="TypeSymbolData"/>.
    /// </summary>
    public static TypeSymbolData Extract(INamedTypeSymbol symbol)
    {
        var data = new TypeSymbolData
        {
            Name = ModelNaming.ApiName(symbol),
            FullName = ModelNaming.ApiFullName(symbol),
            IsInterface = symbol.TypeKind == TypeKind.Interface,
            IsImmutable = HasAttribute(symbol, "ImmutableAttribute"),
            IsInterfaceOnly = HasAttribute(symbol, "InterfaceOnlyAttribute"),
            VmfModelAttribute = GetVmfModelData(symbol),
            VmfEqualsAttribute = GetVmfEqualsData(symbol),
            Documentation = GetDocAttribute(symbol),
        };

        // Base interfaces (only model types — those in the same assembly with VMF attributes)
        foreach (var iface in symbol.Interfaces)
        {
            data.BaseTypeNames.Add(GetFullName(iface));
        }

        // Properties
        foreach (var member in symbol.GetMembers())
        {
            if (member is IPropertySymbol prop && !prop.IsStatic)
            {
                data.Properties.Add(ExtractProperty(prop));
            }
        }

        // Constructor delegation — read first, because a type-level [DelegateTo] also supplies
        // the behaviour class for methods on this type that carry no attribute of their own.
        var ctorDelegation = GetConstructorDelegation(symbol);
        if (ctorDelegation != null)
        {
            data.ConstructorDelegation = ctorDelegation;
        }

        // Delegations
        foreach (var member in symbol.GetMembers())
        {
            if (member is IMethodSymbol method
                && method.MethodKind == MethodKind.Ordinary
                && !method.IsStatic)
            {
                var delegation = ExtractDelegation(method, symbol, ctorDelegation);
                if (delegation != null)
                {
                    data.MethodDelegations.Add(delegation);
                }
            }
        }

        // Custom annotations
        foreach (var attr in symbol.GetAttributes())
        {
            if (IsVmfAnnotationAttribute(attr))
            {
                var ann = ExtractAnnotation(attr);
                if (ann != null) data.Annotations.Add(ann);
            }
        }

        return data;
    }

    private static string? MapOpposite(string? oppositeRef) =>
        oppositeRef == null ? null : ModelNaming.MapOppositeReference(oppositeRef);

    private static PropertySymbolData ExtractProperty(IPropertySymbol prop)
    {
        // Unwrap Nullable<T> (e.g. double?/int?/bool?) so the type is named after its
        // underlying value type rather than the bare static `Nullable` facade. A flag
        // is carried so the templates re-append `?`. .NET's Nullable<T> has no Java analog,
        // so it would otherwise slip through as IsValueType==true but named "Nullable".
        var propType = prop.Type;
        bool isNullableValueType = false;
        if (propType is INamedTypeSymbol nullableSymbol
            && nullableSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && nullableSymbol.TypeArguments.Length == 1)
        {
            isNullableValueType = true;
            propType = nullableSymbol.TypeArguments[0];
        }

        var data = new PropertySymbolData
        {
            Name = prop.Name,
            FullTypeName = GetFullName(propType),
            SimpleTypeName = GetSimpleName(propType),
            TypeNamespace = GetNamespace(propType),
            IsPrimitive = IsValueType(propType),
            IsNullableValueType = isNullableValueType,
            IsCollection = IsCollectionType(propType),
            IsRequired = HasAttribute(prop, "RequiredAttribute") || HasAttribute(prop, "VmfRequiredAttribute"),
            IsIgnoredForEquals = HasAttribute(prop, "IgnoreEqualsAttribute"),
            IsIgnoredForToString = HasAttribute(prop, "IgnoreToStringAttribute"),
            IsGetterOnly = HasAttribute(prop, "GetterOnlyAttribute"),
            DefaultValue = GetDefaultValue(prop),
            OrderIndex = GetOrderIndex(prop),
            Documentation = GetDocAttribute(prop),
        };

        // Collection element type
        if (data.IsCollection && propType is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            var elementType = namedType.TypeArguments[0];
            data.CollectionElementSimpleName = GetSimpleName(elementType);
            data.CollectionElementNamespace = GetNamespace(elementType);
        }

        // Containment
        // Opposites name a type by the name written in the MODEL; map it to the generated one so
        // both "Child.Parent" and the already-prefixed "IChild.Parent" resolve.
        data.ContainsOpposite = MapOpposite(GetContainsOpposite(prop));
        data.ContainerOpposite = MapOpposite(GetContainerOpposite(prop));
        data.RefersOpposite = MapOpposite(GetRefersOpposite(prop));

        // Custom annotations
        foreach (var attr in prop.GetAttributes())
        {
            if (IsVmfAnnotationAttribute(attr))
            {
                var ann = ExtractAnnotation(attr);
                if (ann != null) data.Annotations.Add(ann);
            }
        }

        return data;
    }

    private static DelegationSymbolData? ExtractDelegation(
        IMethodSymbol method, INamedTypeSymbol modelType, DelegationSymbolData? constructorDelegation)
    {
        var attr = FindAttribute(method, "DelegateToAttribute");

        // No attribute of its own: fall back to the type-level one, as Java does. A method with
        // neither is left alone -- in C# it may carry a default interface implementation.
        if (attr == null)
        {
            if (constructorDelegation == null) return null;

            return new DelegationSymbolData
            {
                FullTypeName = constructorDelegation.FullTypeName,
                CallerTypeName = constructorDelegation.CallerTypeName,
                MethodName = method.Name,
                ReturnType = method.ReturnsVoid ? "void" : GetCodeName(method.ReturnType),
                ParamTypes = method.Parameters.Select(p => GetCodeName(p.Type)).ToList(),
                ParamNames = method.Parameters.Select(p => p.Name).ToList(),
                Documentation = GetDocAttribute(method),
            };
        }

        var targetType = attr.ConstructorArguments.Length > 0
            ? attr.ConstructorArguments[0].Value as INamedTypeSymbol
            : null;

        return new DelegationSymbolData
        {
            FullTypeName = targetType != null ? GetFullName(targetType) : "",
            CallerTypeName = ResolveCallerType(targetType, modelType),
            MethodName = method.Name,
            // GetFullName would yield "System.Void", which is not writable in C#; the
            // template also compares against the literal "void" to decide whether to return.
            ReturnType = method.ReturnsVoid ? "void" : GetCodeName(method.ReturnType),
            ParamTypes = method.Parameters.Select(p => GetCodeName(p.Type)).ToList(),
            ParamNames = method.Parameters.Select(p => p.Name).ToList(),
            Documentation = GetDocAttribute(method),
        };
    }

    private static DelegationSymbolData? GetConstructorDelegation(INamedTypeSymbol type)
    {
        var attr = FindAttribute(type, "DelegateToAttribute");
        if (attr == null) return null;

        var targetType = attr.ConstructorArguments.Length > 0
            ? attr.ConstructorArguments[0].Value as INamedTypeSymbol
            : null;

        return new DelegationSymbolData
        {
            FullTypeName = targetType != null ? GetFullName(targetType) : "",
            CallerTypeName = ResolveCallerType(targetType, type),
            // Java calls "on" + simple name + "Instantiated" from the generated constructor; the
            // model's only hook for running code at instantiation.
            MethodName = "On" + ModelTypeInfo.StripInterfacePrefix(type.Name) + "Instantiated",
            ReturnType = "void",
        };
    }

    /// <summary>
    /// Finds the <c>T</c> of the <c>IDelegatedBehavior&lt;T&gt;</c> that <paramref
    /// name="delegateType"/> implements, so the generated code can cast to it before calling
    /// <c>SetCaller</c>. Java gets this for free — the field's declared type carries the parameter
    /// — so reading it off the delegate is what lets a delegate written for a supertype serve
    /// every subtype, exactly as it does there.
    /// <para>
    /// A delegate implementing several picks the one <paramref name="modelType"/> satisfies; the
    /// model type itself is the fallback, which turns an unusable delegate into a compile error at
    /// the cast rather than a silently wrong one.
    /// </para>
    /// </summary>
    private static string ResolveCallerType(INamedTypeSymbol? delegateType, INamedTypeSymbol modelType)
    {
        var fallback = GetFullName(modelType);
        if (delegateType == null) return fallback;

        var candidates = delegateType.AllInterfaces
            .Where(i => i.Name == "IDelegatedBehavior" && i.TypeArguments.Length == 1)
            .Select(i => i.TypeArguments[0])
            .ToList();

        if (candidates.Count == 0) return fallback;

        // The delegate names the GENERATED interface, which is a different symbol from the model
        // type being extracted -- so the two are matched by name, not by symbol identity.
        var reachable = new HashSet<string>(StringComparer.Ordinal) { ModelNaming.ApiFullName(modelType) };
        foreach (var i in modelType.AllInterfaces)
        {
            if (ModelNaming.IsModelType(i)) reachable.Add(ModelNaming.ApiFullName(i));
        }

        var match = candidates.FirstOrDefault(t => reachable.Contains(GetFullName(t)));

        return GetFullName(match ?? candidates[0]);
    }

    // --- Attribute helpers ---

    private static bool HasAttribute(ISymbol symbol, string attrName)
    {
        return symbol.GetAttributes().Any(a => MatchesAttributeName(a, attrName));
    }

    private static AttributeData? FindAttribute(ISymbol symbol, string attrName)
    {
        return symbol.GetAttributes().FirstOrDefault(a => MatchesAttributeName(a, attrName));
    }

    /// <summary>Where VMF's own attributes live. Nothing outside it counts.</summary>
    private const string VmfAttributesNamespace = "VMF.NET.Runtime.Attributes";

    /// <summary>
    /// Matches one of VMF's attributes. The namespace check is not optional: several of our names
    /// collide with the BCL's -- <c>Required</c> (DataAnnotations), <c>DefaultValue</c>
    /// (System.ComponentModel) -- and matching on the simple name alone read those as ours.
    /// </summary>
    private static bool MatchesAttributeName(AttributeData attr, string name)
    {
        var attrClass = attr.AttributeClass;
        if (attrClass == null) return false;

        if (attrClass.ContainingNamespace?.ToDisplayString() != VmfAttributesNamespace) return false;

        var className = attrClass.Name;
        return className == name || className == name.Replace("Attribute", "");
    }


    private static VmfModelData? GetVmfModelData(INamedTypeSymbol symbol)
    {
        var attr = FindAttribute(symbol, "VmfModelAttribute");
        if (attr == null) return null;

        var data = new VmfModelData();
        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "Equality" && named.Value.Value is int val)
            {
                data.Value = (EqualsStrategy)val;
            }
        }
        return data;
    }

    private static VmfEqualsData? GetVmfEqualsData(INamedTypeSymbol symbol)
    {
        var attr = FindAttribute(symbol, "VmfEqualsAttribute");
        if (attr == null) return null;

        var data = new VmfEqualsData();

        // VmfEqualsAttribute takes the strategy as a constructor argument;
        // a named "Equality" is also accepted for symmetry with [VmfModel].
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int ctorVal)
        {
            data.Value = (EqualsStrategy)ctorVal;
        }

        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "Equality" && named.Value.Value is int val)
            {
                data.Value = (EqualsStrategy)val;
            }
        }
        return data;
    }

    private static string? GetDocAttribute(ISymbol symbol)
    {
        var attr = FindAttribute(symbol, "DocAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string doc)
            return doc;
        return null;
    }

    private static string? GetDefaultValue(IPropertySymbol prop)
    {
        var attr = FindAttribute(prop, "DefaultValueAttribute") ?? FindAttribute(prop, "VmfDefaultValueAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string val)
            return val;
        return null;
    }

    private static int? GetOrderIndex(IPropertySymbol prop)
    {
        var attr = FindAttribute(prop, "PropertyOrderAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int idx)
            return idx;
        return null;
    }

    private static string? GetContainsOpposite(IPropertySymbol prop)
    {
        var attr = FindAttribute(prop, "ContainsAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string opp)
            return opp;
        return "";
    }

    private static string? GetContainerOpposite(IPropertySymbol prop)
    {
        var attr = FindAttribute(prop, "ContainerAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string opp)
            return opp;
        return "";
    }

    private static string? GetRefersOpposite(IPropertySymbol prop)
    {
        var attr = FindAttribute(prop, "RefersAttribute");
        if (attr == null) return null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string opp)
            return opp;
        return "";
    }

    private static bool IsVmfAnnotationAttribute(AttributeData attr)
    {
        return MatchesAttributeName(attr, "VmfAnnotationAttribute");
    }

    private static AnnotationData? ExtractAnnotation(AttributeData attr)
    {
        string? key = null;
        string? value = null;

        // VmfAnnotationAttribute(string value) — constructor takes the value, Key is a named property
        if (attr.ConstructorArguments.Length >= 1)
            value = attr.ConstructorArguments[0].Value as string;

        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "Key" && named.Value.Value is string k) key = k;
            if (named.Key == "Value" && named.Value.Value is string v) value = v;
        }

        if (key == null) return null;
        return new AnnotationData { Key = key, Value = value ?? "" };
    }

    // --- Type helpers ---

    /// <summary>
    /// The name generated code uses for a type. A reference to a model type names the GENERATED
    /// interface -- <c>MyApp.VmfModel.Child</c> is written as <c>MyApp.IChild</c> -- because the
    /// model itself is build input and never appears in the emitted API. Everything else passes
    /// through unchanged.
    /// </summary>
    private static string GetFullName(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
            return GetFullName(arrayType.ElementType) + "[]";

        if (ModelNaming.TryMapModelType(type, out var apiFullName, out _))
            return apiFullName;

        // An [ExternalType] stand-in names a type that lives elsewhere; that is the type the
        // generated code must reference, not the stand-in.
        if (ModelNaming.ExternalFullName(type) is { } externalName)
            return externalName;

        var ns = GetNamespace(type);
        if (string.IsNullOrEmpty(ns))
            return type.Name;

        // Handle generic types
        if (type is INamedTypeSymbol named && named.TypeArguments.Length > 0)
        {
            var args = string.Join(", ", named.TypeArguments.Select(GetFullName));
            return $"{ns}.{named.Name}<{args}>";
        }

        return $"{ns}.{type.Name}";
    }

    private static string? GetNamespace(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol standIn && ModelNaming.ExternalTypeNamespaceOf(standIn) is { } externalNamespace)
        {
            return string.IsNullOrEmpty(externalNamespace) ? null : externalNamespace;
        }

        if (type is INamedTypeSymbol named && ModelNaming.IsModelType(named))
        {
            var apiNs = ModelNaming.ApiNamespace(named);
            return apiNs == ModelNaming.GlobalNamespaceName ? null : apiNs;
        }

        if (type.ContainingNamespace == null || type.ContainingNamespace.IsGlobalNamespace)
            return null;
        return type.ContainingNamespace.ToDisplayString();
    }

    /// <summary>
    /// A type as generated code should <b>write</b> it: C# keywords for the framework types
    /// (<c>string</c>, not <c>System.String</c>), model types mapped to their generated names.
    /// <para>
    /// Distinct from <see cref="GetFullName"/>, which also feeds the reflection metadata — that
    /// reports <c>System.String</c>, the faithful counterpart of Java's <c>java.lang.String</c>,
    /// and must keep doing so.
    /// </para>
    /// </summary>
    private static string GetCodeName(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
            return GetCodeName(arrayType.ElementType) + "[]";

        if (ModelNaming.TryMapModelType(type, out var apiFullName, out _))
            return apiFullName;

        if (ModelNaming.ExternalFullName(type) is { } externalName)
            return externalName;

        var keyword = type.SpecialType switch
        {
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Byte => "byte",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Char => "char",
            SpecialType.System_Decimal => "decimal",
            SpecialType.System_Double => "double",
            SpecialType.System_Single => "float",
            SpecialType.System_Int16 => "short",
            SpecialType.System_Int32 => "int",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Object => "object",
            SpecialType.System_String => "string",
            _ => null,
        };
        if (keyword != null) return keyword;

        var ns = GetNamespace(type);
        if (string.IsNullOrEmpty(ns)) return type.Name;

        if (type is INamedTypeSymbol named && named.TypeArguments.Length > 0)
        {
            var args = string.Join(", ", named.TypeArguments.Select(GetCodeName));
            return $"{ns}.{named.Name}<{args}>";
        }

        return $"{ns}.{type.Name}";
    }

    /// <summary>The simple name generated code uses; mapped for model types, as GetFullName is.</summary>
    private static string GetSimpleName(ITypeSymbol type) =>
        ModelNaming.TryMapModelType(type, out _, out var apiSimpleName) ? apiSimpleName : type.Name;

    private static bool IsValueType(ITypeSymbol type)
    {
        return type.IsValueType;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named) return false;
        if (named.TypeArguments.Length == 0) return false;

        var name = named.Name;
        return name == "VList"
            || name == "IList"
            || name == "ICollection"
            || name == "IReadOnlyList"
            || name == "IReadOnlyCollection"
            || name == "List"
            || name == "ObservableCollection";
    }
}
