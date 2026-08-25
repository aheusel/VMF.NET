// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace VMF.NET.Core;

/// <summary>
/// Describes a method or constructor delegation to a behavior class ([DelegateTo]).
/// </summary>
public sealed class DelegationInfo
{
    public DelegationInfo(
        string fullTypeName,
        string methodName,
        string returnType,
        List<string> paramTypes,
        List<string> paramNames,
        bool isConstructorDelegation,
        string? documentation = null,
        string callerTypeName = "")
    {
        FullTypeName = fullTypeName;
        MethodName = methodName;
        ReturnType = returnType;
        ParamTypes = paramTypes;
        ParamNames = paramNames;
        IsConstructorDelegation = isConstructorDelegation;
        Documentation = documentation;
        CallerTypeName = callerTypeName;
    }

    /// <summary>Full type name of the delegation target class.</summary>
    public string FullTypeName { get; }

    /// <summary>Name of the delegated method.</summary>
    public string MethodName { get; }

    /// <summary>Return type of the method.</summary>
    public string ReturnType { get; }

    /// <summary>Parameter types.</summary>
    public List<string> ParamTypes { get; }

    /// <summary>Parameter names.</summary>
    public List<string> ParamNames { get; }

    /// <summary>Whether this is a constructor delegation.</summary>
    public bool IsConstructorDelegation { get; }

    /// <summary>Custom documentation.</summary>
    public string? Documentation { get; }

    /// <summary>Whether this delegation carries documentation.</summary>
    public bool IsDocumented => !string.IsNullOrEmpty(Documentation);

    /// <summary>
    /// The <c>T</c> the delegate class declares its <c>IDelegatedBehavior&lt;T&gt;</c> at, which
    /// is what the generated code casts to before calling <c>SetCaller</c>. Java needs no cast —
    /// the field's own type carries the parameter — so reading <c>T</c> off the delegate is how
    /// the same models compile here: the delegate implements the behaviour interface once, at
    /// whichever model type suits it, and every subtype satisfies the cast by inheritance.
    /// </summary>
    public string CallerTypeName { get; }

    /// <summary>
    /// Field name for the delegation target instance. Java indexes the delegate <em>type</em>, so
    /// one object holds a single instance of each delegate class and shares it between the
    /// constructor hook and every delegated method. A delegate that keeps state across calls
    /// depends on that, so the name is derived from the delegate class rather than the method.
    /// </summary>
    public string VariableName
    {
        get
        {
            // The simple name keeps the field readable; the hash of the full name keeps two
            // delegate classes that share it in different namespaces apart. Deterministic, so the
            // generated source is stable across builds.
            int cut = FullTypeName.LastIndexOf('.');
            var simpleName = cut < 0 ? FullTypeName : FullTypeName.Substring(cut + 1);

            unchecked
            {
                uint hash = 2166136261;
                foreach (var c in FullTypeName)
                {
                    hash = (hash ^ c) * 16777619;
                }
                return $"__vmf_delegate_{simpleName}_{hash:x8}";
            }
        }
    }

    /// <summary>True if this delegation is for interface-only types (no behavior type specified).</summary>
    public bool IsExclusivelyForInterfaceOnlyTypes => string.IsNullOrEmpty(FullTypeName);

    /// <summary>
    /// Java's delegation identity: <c>methodName(paramType1;...;paramTypeN)</c>, or
    /// <c>constructor-(...)</c> for a constructor delegation. Inherited delegations are collected
    /// after the type's own and then reduced to one entry per signature, so a redeclaration in the
    /// concrete type wins — and, since every constructor delegation shares
    /// <c>constructor-()</c>, exactly one of those survives per implementation.
    /// </summary>
    public string MethodSignature
    {
        get
        {
            var name = IsConstructorDelegation ? "constructor-" : MethodName;
            return $"{name}({string.Join(";", ParamTypes)})";
        }
    }
}
