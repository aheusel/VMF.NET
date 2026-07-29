// Copyright 2017-2024 Michael Hoffer <info@michaelhoffer.de>. All rights reserved.
// Copyright 2017-2019 Goethe Center for Scientific Computing, University Frankfurt. All rights reserved.
// Licensed under the Apache License, Version 2.0.

namespace VMF.NET.Json;

/// <summary>
/// Thrown when a schema-driving annotation (<c>vmf:schema:*</c>) carries a malformed value —
/// e.g. a non-numeric <c>minimum</c>, an uncompilable <c>pattern</c>, or invalid <c>inject</c>
/// JSON. Previously such values were silently dropped, leaving the author no feedback; the schema
/// generator now fails fast with an actionable message instead.
/// </summary>
public sealed class VmfSchemaAnnotationException : System.Exception
{
    public VmfSchemaAnnotationException(string message) : base(message) { }

    public VmfSchemaAnnotationException(string message, System.Exception innerException)
        : base(message, innerException) { }
}
