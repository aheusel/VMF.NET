// Model interfaces for testing JSON annotation features.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.IntegrationTests.Models;

/// <summary>
/// A service config with various JSON schema annotations for testing.
/// </summary>
[VmfModel(Equality = EqualsType.All)]
public partial interface IServiceConfig
{
    [VmfAnnotation("service_name", Key = "vmf:jackson:rename")]
    string? Name { get; set; }

    [VmfAnnotation("The port number for the service", Key = "vmf:jackson:schema:description")]
    [VmfAnnotation("minimum=1", Key = "vmf:jackson:schema:constraint")]
    [VmfAnnotation("maximum=65535", Key = "vmf:jackson:schema:constraint")]
    [VmfDefaultValue("8080")]
    int Port { get; set; }

    [VmfAnnotation("hostname", Key = "vmf:jackson:schema:format")]
    [VmfAnnotation("Server Hostname", Key = "vmf:jackson:schema:title")]
    string? Host { get; set; }

    [VmfAnnotation("true", Key = "vmf:jackson:schema:uniqueItems")]
    VList<string> Tags { get; }

    [VmfAnnotation("1", Key = "vmf:jackson:schema:propertyOrder")]
    bool Enabled { get; set; }
}
