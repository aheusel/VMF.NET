// Models exercising Finding 4 Step B: schema-annotation value validation and JSON-array defaults.
// The malformed annotations here are valid C# (annotation values are opaque strings); they only
// surface an error when a JSON schema is generated for the type.
//
// They live in their OWN namespace deliberately. A namespace is one model, and schema generation
// emits definitions for every type in the model, so a type carrying a knowingly-invalid
// annotation would break schema generation for every valid type beside it.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.SchemaValidation.VmfModel;

/// <summary>A JSON-array constraint value (default=[...]) must render as a real JSON array.</summary>
[VmfModel(Equality = EqualsType.All)]
interface ArrayDefaultConfig
{
    [VmfAnnotation("default=[\"alldrop\", \"smartdrop\", \"biospot\"]", Key = "vmf:schema:constraint")]
    string[] DeviceIds { get; }
}

/// <summary>A valid regex pattern constraint — must be accepted.</summary>
[VmfModel(Equality = EqualsType.All)]
interface ValidPatternConfig
{
    [VmfAnnotation("pattern=^\\d{3}$", Key = "vmf:schema:constraint")]
    string? Code { get; set; }
}

/// <summary>An unknown (open-ended) scalar keyword — must still be accepted verbatim.</summary>
[VmfModel(Equality = EqualsType.All)]
interface UnknownKeywordConfig
{
    [VmfAnnotation("deprecated=true", Key = "vmf:schema:constraint")]
    string? Legacy { get; set; }
}

// --- malformed: each must throw VmfSchemaAnnotationException at schema-generation time ---

/// <summary>minimum is not numeric.</summary>
[VmfModel(Equality = EqualsType.All)]
interface BadMinimumConfig
{
    [VmfAnnotation("minimum=abc", Key = "vmf:schema:constraint")]
    int Port { get; set; }
}

/// <summary>pattern is not a compilable regex.</summary>
[VmfModel(Equality = EqualsType.All)]
interface BadPatternConfig
{
    [VmfAnnotation("pattern=[unterminated", Key = "vmf:schema:constraint")]
    string? Code { get; set; }
}

/// <summary>Constraint value is missing the 'keyword=value' form.</summary>
[VmfModel(Equality = EqualsType.All)]
interface BadConstraintFormConfig
{
    [VmfAnnotation("noequalshere", Key = "vmf:schema:constraint")]
    string? Code { get; set; }
}

/// <summary>uniqueItems is not a boolean.</summary>
[VmfModel(Equality = EqualsType.All)]
interface BadUniqueItemsConfig
{
    [VmfAnnotation("yes", Key = "vmf:schema:uniqueItems")]
    string[] Tags { get; }
}

/// <summary>inject is not valid JSON.</summary>
[VmfModel(Equality = EqualsType.All)]
interface BadInjectConfig
{
    [VmfAnnotation("\"examples\": [1, 2,", Key = "vmf:schema:inject")]
    int Value { get; set; }
}
