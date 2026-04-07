// Model interfaces for testing property ordering and doc annotations.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.IntegrationTests.Models;

/// <summary>
/// A config with ordered properties and documentation.
/// </summary>
[Doc("Configuration with ordered properties")]
public partial interface IConfig
{
    [PropertyOrder(2)]
    [Doc("The port number")]
    int Port { get; set; }

    [PropertyOrder(0)]
    [Doc("The host name")]
    string? Host { get; set; }

    [PropertyOrder(1)]
    [Doc("The protocol")]
    string? Protocol { get; set; }
}
