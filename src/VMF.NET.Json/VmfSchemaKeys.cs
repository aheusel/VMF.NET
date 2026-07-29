// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Json;

/// <summary>
/// Annotation keys that drive JSON Schema generation (see <see cref="VmfJsonSchemaGenerator"/>).
/// These names are library-agnostic: VMF.NET uses System.Text.Json, so the keys deliberately do
/// not name any serializer. They mirror the constant-based treatment the core
/// <c>vmf:change:type:*</c> keys already get in <c>ChangeTypeConstants</c>.
/// </summary>
public static class VmfSchemaKeys
{
    public const string Description = "vmf:schema:description";
    public const string Title = "vmf:schema:title";
    public const string Format = "vmf:schema:format";
    public const string Constraint = "vmf:schema:constraint";
    public const string UniqueItems = "vmf:schema:uniqueItems";
    public const string PropertyOrder = "vmf:schema:propertyOrder";
    public const string Inject = "vmf:schema:inject";
}

/// <summary>
/// Annotation keys that control JSON serialization behavior (see <see cref="VmfJsonConverter"/>).
/// </summary>
public static class VmfJsonKeys
{
    /// <summary>Renames a property's serialized field name.</summary>
    public const string Name = "vmf:json:name";
}
