// Model interfaces for testing JSON annotation features.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

/// <summary>
/// A service config with various JSON schema annotations for testing.
/// </summary>
[VmfModel(Equality = EqualsType.All)]
interface IServiceConfig
{
    [VmfAnnotation("service_name", Key = "vmf:json:name")]
    string? Name { get; set; }

    [VmfAnnotation("The port number for the service", Key = "vmf:schema:description")]
    [VmfAnnotation("minimum=1", Key = "vmf:schema:constraint")]
    [VmfAnnotation("maximum=65535", Key = "vmf:schema:constraint")]
    [VmfDefaultValue("8080")]
    int Port { get; set; }

    [VmfAnnotation("hostname", Key = "vmf:schema:format")]
    [VmfAnnotation("Server Hostname", Key = "vmf:schema:title")]
    string? Host { get; set; }

    [VmfAnnotation("true", Key = "vmf:schema:uniqueItems")]
    string[] Tags { get; }

    [VmfAnnotation("1", Key = "vmf:schema:propertyOrder")]
    bool Enabled { get; set; }
}
