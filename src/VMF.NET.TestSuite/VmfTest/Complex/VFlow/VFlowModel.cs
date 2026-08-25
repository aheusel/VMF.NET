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
interface IWithLocation
{
    [VmfDefaultValue("0")] int? X { get; set; }
    [VmfDefaultValue("0")] int? Y { get; set; }
}

[InterfaceOnly]
interface IWithDimensions
{
    [VmfDefaultValue("0")] int? Width { get; set; }
    [VmfDefaultValue("0")] int? Height { get; set; }
}

[InterfaceOnly]
interface IWithId
{
    string? Id { get; set; }
}

[InterfaceOnly]
interface IWithName
{
    string? Name { get; set; }
}

[InterfaceOnly]
interface IWithType
{
    [VmfDefaultValue("\"default\"")] string? Type { get; set; }
}

[InterfaceOnly]
interface IWithValue
{
    object? Value { get; set; }
}

[InterfaceOnly]
[DelegateTo(typeof(ConnectorDelegate))]
interface IConnector : IWithId, IWithType, IWithValue
{
    [GetterOnly] IVNode? Parent { get; }

    [GetterOnly] VList<IConnection> Connections { get; }

    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? TryConnect(IConnector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? Connect(IConnector c);
}

interface IInput : IConnector
{
    [Container("IVNode.Inputs")]
    new IVNode? Parent { get; }

    [Contains("IConnection.Receiver")]
    new VList<IConnection> Connections { get; }
}

interface IOutput : IConnector
{
    [Container("IVNode.Outputs")]
    new IVNode? Parent { get; }

    [Contains("IConnection.Sender")]
    new VList<IConnection> Connections { get; }
}

[DelegateTo(typeof(ConnectionDelegate))]
interface IConnection : IWithId, IWithType
{
    // Settable, because ConnectorDelegate.Connect assigns them. Java generates a container
    // setter automatically; here a model opts in by declaring the property `{ get; set; }`.
    [Container("IOutput.Connections")]
    IOutput? Sender { get; set; }

    [Container("IInput.Connections")]
    IInput? Receiver { get; set; }

    [Container("IVFlow.Connections")]
    IVFlow? Flow { get; }
}

interface IConnectionResult
{
    IConnection? Connection { get; set; }
    bool Successful { get; set; }
    [VmfDefaultValue("\"\"")] string? Message { get; set; }
}

interface IVNode : IWithLocation, IWithDimensions, IWithId, IWithType, IWithValue, IWithName
{
    [Contains("IInput.Parent")]
    VList<IInput> Inputs { get; }

    [Contains("IOutput.Parent")]
    VList<IOutput> Outputs { get; }

    [Container("IVFlow.Nodes")]
    IVFlow? Parent { get; }

    [DelegateTo(typeof(VNodeDelegate))]
    IInput? AddInput(string type);

    [DelegateTo(typeof(VNodeDelegate))]
    IOutput? AddOutput(string type);
}

[DelegateTo(typeof(VFlowDelegate))]
interface IVFlow : IVNode
{
    [Contains("IVNode.Parent")]
    VList<IVNode> Nodes { get; }

    [Contains("IConnection.Flow")]
    VList<IConnection> Connections { get; }

    [DelegateTo(typeof(VFlowDelegate))]
    IConnectionResult? Connect(IConnector c1, IConnector c2);

    [DelegateTo(typeof(VFlowDelegate))]
    IConnectionResult? TryConnect(IConnector c1, IConnector c2);

    [DelegateTo(typeof(VFlowDelegate))]
    IConnectionResult? Connect(IVNode n1, IVNode n2, string type);

    [DelegateTo(typeof(VFlowDelegate))]
    IVNode? NewNode(object o);

    [DelegateTo(typeof(VFlowDelegate))]
    IVFlow? NewSubFlow(object o);
}
