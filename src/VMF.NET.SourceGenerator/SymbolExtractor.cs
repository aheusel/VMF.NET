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
            Name = symbol.Name,
            FullName = GetFullName(symbol),
            IsInterface = symbol.TypeKind == TypeKind.Interface,
            IsImmutable = HasAttribute(symbol, "ImmutableAttribute"),
            IsInterfaceOnly = HasAttribute(symbol, "InterfaceOnlyAttribute"),
            ExternalTypeNamespace = GetExternalTypeNamespace(symbol),
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

        // Delegations
        foreach (var member in symbol.GetMembers())
        {
            if (member is IMethodSymbol method
                && method.MethodKind == MethodKind.Ordinary
                && !method.IsStatic)
            {
                var delegation = ExtractDelegation(method);
                if (delegation != null)
                {
                    data.MethodDelegations.Add(delegation);
                }
            }
        }

        // Constructor delegation
        var ctorDelegation = GetConstructorDelegation(symbol);
        if (ctorDelegation != null)
        {
            data.ConstructorDelegation = ctorDelegation;
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

    private static PropertySymbolData ExtractProperty(IPropertySymbol prop)
    {
        var data = new PropertySymbolData
        {
            Name = prop.Name,
            FullTypeName = GetFullName(prop.Type),
            SimpleTypeName = prop.Type.Name,
            TypeNamespace = GetNamespace(prop.Type),
            IsPrimitive = IsValueType(prop.Type),
            IsCollection = IsCollectionType(prop.Type),
            IsRequired = HasAttribute(prop, "RequiredAttribute") || HasAttribute(prop, "VmfRequiredAttribute"),
            IsIgnoredForEquals = HasAttribute(prop, "IgnoreEqualsAttribute"),
            IsIgnoredForToString = HasAttribute(prop, "IgnoreToStringAttribute"),
            IsGetterOnly = HasAttribute(prop, "GetterOnlyAttribute"),
            DefaultValue = GetDefaultValue(prop),
            OrderIndex = GetOrderIndex(prop),
            Documentation = GetDocAttribute(prop),
        };

        // Collection element type
        if (data.IsCollection && prop.Type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            var elementType = namedType.TypeArguments[0];
            data.CollectionElementSimpleName = elementType.Name;
            data.CollectionElementNamespace = GetNamespace(elementType);
        }

        // Containment
        data.ContainsOpposite = GetContainsOpposite(prop);
        data.ContainerOpposite = GetContainerOpposite(prop);
        data.RefersOpposite = GetRefersOpposite(prop);

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

    private static DelegationSymbolData? ExtractDelegation(IMethodSymbol method)
    {
        var attr = FindAttribute(method, "DelegateToAttribute");
        if (attr == null) return null;

        var targetType = attr.ConstructorArguments.Length > 0
            ? attr.ConstructorArguments[0].Value as INamedTypeSymbol
            : null;

        return new DelegationSymbolData
        {
            FullTypeName = targetType != null ? GetFullName(targetType) : "",
            MethodName = method.Name,
            ReturnType = GetFullName(method.ReturnType),
            ParamTypes = method.Parameters.Select(p => GetFullName(p.Type)).ToList(),
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
            MethodName = "",
            ReturnType = "void",
        };
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

    private static bool MatchesAttributeName(AttributeData attr, string name)
    {
        var className = attr.AttributeClass?.Name;
        if (className == null) return false;
        return className == name || className == name.Replace("Attribute", "");
    }

    private static string? GetExternalTypeNamespace(INamedTypeSymbol symbol)
    {
        var attr = FindAttribute(symbol, "ExternalTypeAttribute");
        if (attr == null) return null;

        // ExternalType has a Namespace property
        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "Namespace" && named.Value.Value is string ns)
                return ns;
        }
        // Or first constructor arg
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string ctorNs)
            return ctorNs;

        return symbol.ContainingNamespace?.ToDisplayString() ?? "";
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

    private static string GetFullName(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
            return GetFullName(arrayType.ElementType) + "[]";

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
        if (type.ContainingNamespace == null || type.ContainingNamespace.IsGlobalNamespace)
            return null;
        return type.ContainingNamespace.ToDisplayString();
    }

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
