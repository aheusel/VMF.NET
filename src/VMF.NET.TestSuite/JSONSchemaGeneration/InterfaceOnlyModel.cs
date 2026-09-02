// Repro for a 0.3.2 regression: an [InterfaceOnly] type anywhere in the model's namespace made
// schema generation throw.
//
// GetSubTypes walks every type in the namespace asking each for its SuperTypes(), and SuperTypes()
// needs a prototype -- which an interface-only type, being non-instantiable, does not have. The
// failure therefore has nothing to do with the property being described: merely declaring an
// interface-only type in the same namespace was enough to break any model-typed property.

using VMF.NET.Json;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.JSONSchemaGeneration.InterfaceOnlyCase.VmfModel;

[InterfaceOnly]
interface ConfigElement
{
}

interface AppConfig : ConfigElement
{
    string? Title { get; set; }

    [Container("ConfigHolder.Apps")]
    ConfigHolder? Holder { get; }
}

interface ConfigHolder
{
    [Contains("AppConfig.Holder")]
    AppConfig[] Apps { get; }

    [Contains("PiezoChannelConfig.Holder")]
    PiezoChannelConfig[] Channels { get; }
}

// Re-declaring an inherited property in a derived interface, to give it its own annotations.
// Java allows the redeclaration outright; C# warns CS0108 unless the intent is stated with
// `new`. The question this pins is whether the DERIVED declaration's annotations are the ones
// that reach the schema.
[InterfaceOnly]
interface WithChannelId
{
    int ChannelId { get; set; }
}

interface PiezoChannelConfig : WithChannelId, ConfigElement
{
    [VmfAnnotation("\"title\": \"Channel ID\", \"propertyOrder\": 0", Key = VmfSchemaKeys.Inject)]
    new int ChannelId { get; set; }

    [Container("ConfigHolder.Channels")]
    ConfigHolder? Holder { get; }
}
