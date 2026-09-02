// Repro for a 0.3.2 regression: an [InterfaceOnly] type anywhere in the model's namespace made
// schema generation throw.
//
// GetSubTypes walks every type in the namespace asking each for its SuperTypes(), and SuperTypes()
// needs a prototype -- which an interface-only type, being non-instantiable, does not have. The
// failure therefore has nothing to do with the property being described: merely declaring an
// interface-only type in the same namespace was enough to break any model-typed property.

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
}
