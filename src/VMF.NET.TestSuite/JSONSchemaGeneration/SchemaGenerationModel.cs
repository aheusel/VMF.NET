// Model for JSON Schema generation, ported from Java's own schema test model:
//   VMF/jackson/src/test/vmf/eu/mihosoft/vmf/jackson/test/simple/vmfmodel/PolymorphicModel.java
//
// Kept deliberately close to the Java original -- MyModel/Person/Employee/Address, the same
// annotations on the same properties -- so the generated schema can be compared against what
// Java's VMFJsonSchemaGenerator produces rather than against our own expectations.
//
// ServiceEndpoint is the one addition: Java has renames covered elsewhere, and we need one here
// to pin that a rename is never run through a naming policy.

using VMF.NET.Json;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.JSONSchemaGeneration.VmfModel;

interface MyModel
{
    // Declared as Person[] but holds Employee instances too -- this is what makes the schema
    // polymorphic, and what oneOf has to describe.
    [Contains("Person.Model")]
    Person[] Persons { get; }
}

interface Person
{
    string? Name { get; set; }

    [VmfDefaultValue("30")]
    [VmfAnnotation("minimum=0", Key = VmfSchemaKeys.Constraint)]
    [VmfAnnotation("maximum=99", Key = VmfSchemaKeys.Constraint)]
    int Age { get; set; }

    [VmfAnnotation("Residential address of the person.", Key = VmfSchemaKeys.Description)]
    Address? Address { get; set; }

    [Container("MyModel.Persons")]
    MyModel? Model { get; }
}

interface Employee : Person
{
    [VmfAnnotation("Employee ID.", Key = VmfSchemaKeys.Description)]
    string? EmployeeId { get; set; }
}

[Immutable]
interface Address
{
    string? Street { get; }
    string? City { get; }

    [VmfDefaultValue("\"10000\"")]
    string? Zip { get; }
}

interface ServiceEndpoint
{
    [VmfAnnotation("service_name", Key = VmfJsonKeys.Name)]
    string? Name { get; set; }

    string? HostName { get; set; }
}

// json-editor hints. https://github.com/json-editor/json-editor reads these straight out of the
// schema -- `format` tells it which widget to draw, `propertyOrder` where to put the field. The
// annotations below are the exact ones a Java VMF model would carry, with only the key prefix
// changed (`vmf:jackson:schema:*` became `vmf:schema:*` in VMF.NET 0.2.0).
interface CameraConfig
{
    [VmfAnnotation("The cameras frames-per-second value.", Key = VmfSchemaKeys.Description)]
    int Fps { get; set; }

    [VmfAnnotation("checkbox", Key = VmfSchemaKeys.Format)]
    bool Enabled { get; set; }

    [VmfAnnotation("\"title\": \"Installed Pipe ID\", \"propertyOrder\": 17", Key = VmfSchemaKeys.Inject)]
    string? PipeId { get; set; }

    // Same keyword from two directions. Java applies injections BEFORE title, so the explicit
    // annotation wins; this property exists to keep it that way.
    [VmfAnnotation("\"title\": \"from inject\"", Key = VmfSchemaKeys.Inject)]
    [VmfAnnotation("from annotation", Key = VmfSchemaKeys.Title)]
    string? Label { get; set; }
}
