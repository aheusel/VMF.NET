// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.CodeAnalysis;
using VMF.NET.Core;

namespace VMF.NET.SourceGenerator;

/// <summary>
/// The model-namespace convention.
/// <para>
/// A model type is an interface declared in a namespace whose last segment is
/// <c>VmfModel</c>, and its public counterpart is generated into that namespace's <b>parent</b>.
/// So <c>MyApp.VmfModel.Parent</c> produces <c>MyApp.IParent</c>.
/// </para>
/// <para>
/// This mirrors Java, where the model lives in a <c>vmfmodel</c> package and VMF generates into
/// the package above it. Being there is the whole declaration — no attribute marks a model type,
/// which is what makes a plain <c>interface Named { string Name { get; set; } }</c> work, and what
/// stops an unrelated interface elsewhere from being mistaken for one.
/// </para>
/// </summary>
internal static class ModelNaming
{
    /// <summary>Fallback namespace name for a model declared at global scope.</summary>
    public const string GlobalNamespaceName = "Global";

    public static bool IsModelType(INamedTypeSymbol symbol) =>
        symbol.TypeKind == TypeKind.Interface
        && IsModelNamespace(symbol.ContainingNamespace)
        && ExternalTypeNamespaceOf(symbol) == null;

    /// <summary>
    /// The namespace named by <c>[ExternalType("…")]</c>, or null if the interface does not carry
    /// it.
    /// <para>
    /// Such an interface is a **stand-in**, not a model type: it names a type that lives outside
    /// the model, and generated code must reference that type rather than the stand-in. Java needs
    /// the same device because its model package is compiled on its own.
    /// </para>
    /// </summary>
    public static string? ExternalTypeNamespaceOf(INamedTypeSymbol symbol)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass?.Name is not ("ExternalTypeAttribute" or "ExternalType")) continue;
            if (attrClass.ContainingNamespace?.ToDisplayString() != "VMF.NET.Runtime.Attributes") continue;

            if (attr.ConstructorArguments.Length > 0
                && attr.ConstructorArguments[0].Value is string ctorNamespace)
            {
                return ctorNamespace;
            }

            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "Namespace" && named.Value.Value is string namedNamespace)
                    return namedNamespace;
            }

            return "";
        }

        return null;
    }

    /// <summary>
    /// The name generated code uses for an <c>[ExternalType]</c> stand-in: the external namespace
    /// it names, plus the stand-in's own simple name. Null if it is not a stand-in.
    /// </summary>
    public static string? ExternalFullName(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named) return null;

        var externalNamespace = ExternalTypeNamespaceOf(named);
        if (externalNamespace == null) return null;

        return string.IsNullOrEmpty(externalNamespace) ? named.Name : $"{externalNamespace}.{named.Name}";
    }

    public static bool IsModelNamespace(INamespaceSymbol? ns) =>
        ns is not null
        && !ns.IsGlobalNamespace
        && ns.Name == ModelTypeInfo.ModelNamespaceSegment;

    /// <summary>The namespace the public API is generated into: the model namespace's parent.</summary>
    public static string ApiNamespace(INamedTypeSymbol symbol) =>
        ApiNamespace(symbol.ContainingNamespace);

    public static string ApiNamespace(INamespaceSymbol modelNamespace)
    {
        var parent = modelNamespace.ContainingNamespace;
        return parent is null || parent.IsGlobalNamespace
            ? GlobalNamespaceName
            : parent.ToDisplayString();
    }

    /// <summary>The generated interface's simple name.</summary>
    public static string ApiName(INamedTypeSymbol symbol) =>
        ModelTypeInfo.ApiInterfaceName(symbol.Name);

    /// <summary>The generated interface's full name, which is how the model refers to it.</summary>
    public static string ApiFullName(INamedTypeSymbol symbol)
    {
        var ns = ApiNamespace(symbol);
        return ns == GlobalNamespaceName ? ApiName(symbol) : $"{ns}.{ApiName(symbol)}";
    }

    /// <summary>
    /// Maps a type reference written in a model file to the name the generated code uses. Model
    /// types are renamed and re-homed; everything else — <c>string</c>, an enum, a BCL type — is
    /// passed through untouched.
    /// </summary>
    public static bool TryMapModelType(ITypeSymbol? type, out string apiFullName, out string apiSimpleName)
    {
        if (type is INamedTypeSymbol named && IsModelType(named))
        {
            apiFullName = ApiFullName(named);
            apiSimpleName = ApiName(named);
            return true;
        }

        apiFullName = "";
        apiSimpleName = "";
        return false;
    }

    /// <summary>
    /// Maps the type part of an opposite reference (<c>"Child.Parent"</c>, or a bare
    /// <c>"Parent"</c>) so it names the generated type. A reference already written in the
    /// generated form (<c>"IChild.Parent"</c>) is left as it is, because
    /// <see cref="ModelTypeInfo.ApiInterfaceName"/> is idempotent on names that already carry the
    /// prefix.
    /// </summary>
    public static string MapOppositeReference(string oppositeRef)
    {
        if (string.IsNullOrEmpty(oppositeRef)) return oppositeRef;

        int cut = oppositeRef.LastIndexOf('.');
        if (cut < 0) return oppositeRef;   // bare property name — no type part to map

        var typePart = oppositeRef.Substring(0, cut);
        var propPart = oppositeRef.Substring(cut + 1);

        // Only the last segment of the type part is a simple type name; anything before it is a
        // namespace and must be left alone.
        int nsCut = typePart.LastIndexOf('.');
        if (nsCut < 0) return $"{ModelTypeInfo.ApiInterfaceName(typePart)}.{propPart}";

        var ns = typePart.Substring(0, nsCut);
        var simple = typePart.Substring(nsCut + 1);
        return $"{ns}.{ModelTypeInfo.ApiInterfaceName(simple)}.{propPart}";
    }
}
