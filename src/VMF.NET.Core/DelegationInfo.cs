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
        string? documentation = null)
    {
        FullTypeName = fullTypeName;
        MethodName = methodName;
        ReturnType = returnType;
        ParamTypes = paramTypes;
        ParamNames = paramNames;
        IsConstructorDelegation = isConstructorDelegation;
        Documentation = documentation;
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

    /// <summary>Variable name for the delegation target instance.</summary>
    public string VariableName => $"__vmf_delegate_{MethodName}";

    /// <summary>True if this delegation is for interface-only types (no behavior type specified).</summary>
    public bool IsExclusivelyForInterfaceOnlyTypes => string.IsNullOrEmpty(FullTypeName);

    /// <summary>Method signature string for display.</summary>
    public string MethodSignature
    {
        get
        {
            var paramStr = string.Join(", ", ParamTypes);
            return $"{ReturnType} {MethodName}({paramStr})";
        }
    }
}
