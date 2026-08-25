// Ported from eu.mihosoft.vmftest.complex.vflow.vmfmodel.VFlow and the four behaviour
// delegates beside it.
//
// The `new` keywords are C#: Input/Output re-declare Connector's Parent and Connections to
// attach [Container]/[Contains], which hides the base member rather than overriding it.

using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VFlow.VmfModel;

[InterfaceOnly]
interface WithLocation
{
    [VmfDefaultValue("0")] int? X { get; set; }
    [VmfDefaultValue("0")] int? Y { get; set; }
}

[InterfaceOnly]
interface WithDimensions
{
    [VmfDefaultValue("0")] int? Width { get; set; }
    [VmfDefaultValue("0")] int? Height { get; set; }
}

[InterfaceOnly]
interface WithId
{
    string? Id { get; set; }
}

[InterfaceOnly]
interface WithName
{
    string? Name { get; set; }
}

[InterfaceOnly]
interface WithType
{
    [VmfDefaultValue("\"default\"")] string? Type { get; set; }
}

[InterfaceOnly]
interface WithValue
{
    object? Value { get; set; }
}

[InterfaceOnly]
[DelegateTo(typeof(ConnectorDelegate))]
interface Connector : WithId, WithType, WithValue
{
    [GetterOnly] VNode? Parent { get; }

    [GetterOnly] Connection[] Connections { get; }

    [DelegateTo(typeof(ConnectorDelegate))]
    ConnectionResult? TryConnect(Connector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    ConnectionResult? Connect(Connector c);
}

interface Input : Connector
{
    [Container("VNode.Inputs")]
    new VNode? Parent { get; }

    [Contains("Connection.Receiver")]
    new Connection[] Connections { get; }
}

interface Output : Connector
{
    [Container("VNode.Outputs")]
    new VNode? Parent { get; }

    [Contains("Connection.Sender")]
    new Connection[] Connections { get; }
}

[DelegateTo(typeof(ConnectionDelegate))]
interface Connection : WithId, WithType
{
    // Settable, because ConnectorDelegate.Connect assigns them. Java generates a container
    // setter automatically; here a model opts in by declaring the property `{ get; set; }`.
    [Container("Output.Connections")]
    Output? Sender { get; set; }

    [Container("Input.Connections")]
    Input? Receiver { get; set; }

    [Container("VFlow.Connections")]
    VFlow? Flow { get; }
}

interface ConnectionResult
{
    Connection? Connection { get; set; }
    bool Successful { get; set; }
    [VmfDefaultValue("\"\"")] string? Message { get; set; }
}

interface VNode : WithLocation, WithDimensions, WithId, WithType, WithValue, WithName
{
    [Contains("Input.Parent")]
    Input[] Inputs { get; }

    [Contains("Output.Parent")]
    Output[] Outputs { get; }

    [Container("VFlow.Nodes")]
    VFlow? Parent { get; }

    [DelegateTo(typeof(VNodeDelegate))]
    Input? AddInput(string type);

    [DelegateTo(typeof(VNodeDelegate))]
    Output? AddOutput(string type);
}

[DelegateTo(typeof(VFlowDelegate))]
interface VFlow : VNode
{
    [Contains("VNode.Parent")]
    VNode[] Nodes { get; }

    [Contains("Connection.Flow")]
    Connection[] Connections { get; }

    [DelegateTo(typeof(VFlowDelegate))]
    ConnectionResult? Connect(Connector c1, Connector c2);

    [DelegateTo(typeof(VFlowDelegate))]
    ConnectionResult? TryConnect(Connector c1, Connector c2);

    [DelegateTo(typeof(VFlowDelegate))]
    ConnectionResult? Connect(VNode n1, VNode n2, string type);

    [DelegateTo(typeof(VFlowDelegate))]
    VNode? NewNode(object o);

    [DelegateTo(typeof(VFlowDelegate))]
    VFlow? NewSubFlow(object o);
}
