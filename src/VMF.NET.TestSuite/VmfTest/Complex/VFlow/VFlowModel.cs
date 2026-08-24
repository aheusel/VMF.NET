// Ported from eu.mihosoft.vmftest.complex.vflow.vmfmodel.VFlow and the four behaviour
// delegates beside it.
//
// DEVIATIONS:
//  1. A `new` redeclaration of a property leaves the BASE read-only interface member
//     unimplemented, so Parent/Connections are declared only on Input/Output, not on
//     Connector as in Java. That is also why OnConnectorInstantiated has to ask which of the
//     two it is holding before it can reach Connections.
//  2. ConnectorDelegate.TryConnect/Connect return null instead of Java's connection logic,
//     which needs Connector.Parent -- unavailable here for the reason above. No ported fact
//     calls either method.

using System.Linq;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VFlow;

[VmfModel]
[InterfaceOnly]
public partial interface IWithLocation
{
    [VmfDefaultValue("0")] int? X { get; set; }
    [VmfDefaultValue("0")] int? Y { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithDimensions
{
    [VmfDefaultValue("0")] int? Width { get; set; }
    [VmfDefaultValue("0")] int? Height { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithId
{
    string? Id { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithName
{
    string? Name { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithType
{
    [VmfDefaultValue("\"default\"")] string? Type { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithValue
{
    object? Value { get; set; }
}

[VmfModel]
[InterfaceOnly]
[DelegateTo(typeof(ConnectorDelegate))]
public partial interface IConnector : IWithId, IWithType, IWithValue
{
    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? TryConnect(IConnector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? Connect(IConnector c);
}

[VmfModel]
public partial interface IInput : IConnector
{
    [Container("IVNode.Inputs")]
    IVNode? Parent { get; }

    [Contains("IConnection.Receiver")]
    VList<IConnection> Connections { get; }
}

[VmfModel]
public partial interface IOutput : IConnector
{
    [Container("IVNode.Outputs")]
    IVNode? Parent { get; }

    [Contains("IConnection.Sender")]
    VList<IConnection> Connections { get; }
}

[VmfModel]
[DelegateTo(typeof(ConnectionDelegate))]
public partial interface IConnection : IWithId, IWithType
{
    [Container("IOutput.Connections")]
    IOutput? Sender { get; }

    [Container("IInput.Connections")]
    IInput? Receiver { get; }

    [Container("IVFlow.Connections")]
    IVFlow? Flow { get; }
}

[VmfModel]
public partial interface IConnectionResult
{
    IConnection? Connection { get; set; }
    bool Successful { get; set; }
    [VmfDefaultValue("\"\"")] string? Message { get; set; }
}

[VmfModel]
public partial interface IVNode : IWithLocation, IWithDimensions, IWithId, IWithType, IWithValue, IWithName
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

[VmfModel]
[DelegateTo(typeof(VFlowDelegate))]
public partial interface IVFlow : IVNode
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

// --- behavior delegates ---

public sealed class ConnectorDelegate : IDelegatedBehavior<IConnector>
{
    private IConnector? _caller;
    public void SetCaller(IConnector caller) => _caller = caller;

    public void OnConnectorInstantiated()
    {
        // Connections lives on Input/Output here rather than on Connector -- see deviation 1.
        var connections = _caller switch
        {
            IInput input => input.Connections,
            IOutput output => output.Connections,
            _ => null,
        };

        if (connections == null) return;

        // prevent duplicates & set id
        connections.AddChangeListener(evt =>
        {
            foreach (IConnection cnn in evt.Added.Cast<IConnection>())
            {
                if (connections.Count(cnn2 => ReferenceEquals(cnn, cnn2)) > 1)
                {
                    throw new System.InvalidOperationException("Duplicate connections added: " + cnn);
                }
            }
        });
    }

    public IConnectionResult? TryConnect(IConnector c) => null;
    public IConnectionResult? Connect(IConnector c) => null;
}

public sealed class ConnectionDelegate : IDelegatedBehavior<IConnection>
{
    private IConnection? _caller;
    public void SetCaller(IConnection caller) => _caller = caller;

    public void OnConnectionInstantiated()
    {
        // Java's changePermitted flag is omitted: the only code that reads it -- a guard
        // rejecting manual writes to 'id' -- is commented out there.
        _caller!.Vmf().Reflect().PropertyByName("Sender")?.AddChangeListener(_ => SyncId());
        _caller!.Vmf().Reflect().PropertyByName("Receiver")?.AddChangeListener(_ => SyncId());
    }

    private void SyncId()
    {
        string senderId = "<none>";
        if (_caller!.Sender != null && _caller.Sender.Id != null) senderId = _caller.Sender.Id;
        string receiverId = "<none>";
        if (_caller.Receiver != null && _caller.Receiver.Id != null) receiverId = _caller.Receiver.Id;

        _caller.Id = senderId + " -> " + receiverId;
    }
}

public sealed class VNodeDelegate : IDelegatedBehavior<IVNode>
{
    private IVNode? _caller;
    public void SetCaller(IVNode caller) => _caller = caller;

    public IInput? AddInput(string type)
    {
        var input = IInput.NewBuilder().WithType(type).Build();
        _caller!.Inputs.Add(input);
        return input;
    }

    public IOutput? AddOutput(string type)
    {
        var outputs = IOutput.NewBuilder().WithType(type).Build();
        _caller!.Outputs.Add(outputs);
        return outputs;
    }
}

public sealed class VFlowDelegate : IDelegatedBehavior<IVFlow>
{
    private IVFlow? _caller;
    public void SetCaller(IVFlow caller) => _caller = caller;

    public void OnVFlowInstantiated()
    {
        // prevent duplicates & set id
        _caller!.Nodes.AddChangeListener(evt =>
        {
            foreach (IVNode n in evt.Added.Cast<IVNode>())
            {
                if (_caller.Nodes.Count(m => ReferenceEquals(n, m)) > 1)
                {
                    throw new System.InvalidOperationException("Duplicate nodes added: " + n);
                }
            }
        });
    }

    public IConnectionResult? Connect(IConnector c1, IConnector c2) => null;
    public IConnectionResult? TryConnect(IConnector c1, IConnector c2) => null;
    public IConnectionResult? Connect(IVNode n1, IVNode n2, string type) => null;

    public IVNode? NewNode(object o)
    {
        var n = IVNode.NewInstance();
        n.Value = o;
        _caller!.Nodes.Add(n);
        return n;
    }

    public IVFlow? NewSubFlow(object o)
    {
        var f = IVFlow.NewInstance();
        f.Value = o;
        _caller!.Nodes.Add(f);
        return f;
    }
}
