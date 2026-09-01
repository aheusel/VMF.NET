// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

using System.Text.Json;

namespace VMF.NET.Json;

/// <summary>
/// Field naming for VMF.NET JSON documents, shared by the serializer and the schema generator so
/// the two cannot drift apart.
/// </summary>
public static class VmfJsonNaming
{
    /// <summary>
    /// The default field naming, chosen so a VMF.NET document has the same shape as a Java VMF
    /// one.
    /// <para>
    /// Java's field name is simply the model property name — <c>getName()</c> yields the property
    /// <c>name</c>, lower-camel by Java convention — and the Jackson module applies no naming
    /// strategy on top of it. C# property names are PascalCase, so camelCase conversion is what
    /// reproduces Java's document rather than a stylistic preference.
    /// </para>
    /// </summary>
    public static JsonNamingPolicy Default { get; } = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// The policy in force for a set of options: an explicitly configured
    /// <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> wins, otherwise
    /// <see cref="Default"/>.
    /// </summary>
    public static JsonNamingPolicy Resolve(JsonSerializerOptions? options)
        => options?.PropertyNamingPolicy ?? Default;
}
