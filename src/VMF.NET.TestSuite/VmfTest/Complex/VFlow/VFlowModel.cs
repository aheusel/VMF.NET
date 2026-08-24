// Ported from eu.mihosoft.vmftest.complex.vflow.vmfmodel.VFlow and the four behaviour
// delegates beside it.
//
// The `new` keywords are C#: Input/Output re-declare Connector's Parent and Connections to
// attach [Container]/[Contains], which hides the base member rather than overriding it.

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
    [GetterOnly] IVNode? Parent { get; }

    [GetterOnly] VList<IConnection> Connections { get; }

    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? TryConnect(IConnector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    IConnectionResult? Connect(IConnector c);
}

[VmfModel]
public partial interface IInput : IConnector
{
    [Container("IVNode.Inputs")]
    new IVNode? Parent { get; }

    [Contains("IConnection.Receiver")]
    new VList<IConnection> Connections { get; }
}

[VmfModel]
public partial interface IOutput : IConnector
{
    [Container("IVNode.Outputs")]
    new IVNode? Parent { get; }

    [Contains("IConnection.Sender")]
    new VList<IConnection> Connections { get; }
}

[VmfModel]
[DelegateTo(typeof(ConnectionDelegate))]
public partial interface IConnection : IWithId, IWithType
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
        // prevent duplicates & set id
        _caller!.Connections.AddChangeListener(evt =>
        {
            foreach (IConnection cnn in evt.Added.Cast<IConnection>())
            {
                if (_caller.Connections.Count(cnn2 => ReferenceEquals(cnn, cnn2)) > 1)
                {
                    throw new System.InvalidOperationException("Duplicate connections added: " + cnn);
                }
            }
        });
    }

    private sealed class ConnectorTuple
    {
        public IInput? Input;
        public IOutput? Output;

        public ConnectorTuple(IInput? input, IOutput? output)
        {
            Input = input;
            Output = output;
        }
    }

    private ConnectorTuple Sort(IConnector? c1, IConnector? c2)
    {
        IInput? input = null;
        IOutput? output = null;

        if (c1 is IInput && c2 is IOutput)
        {
            input = (IInput)c1;
            output = (IOutput)c2;
        }
        else if (c1 is IOutput && c2 is IInput)
        {
            input = (IInput)c2;
            output = (IOutput)c1;
        }

        return new ConnectorTuple(input, output);
    }

    public IConnectionResult? Connect(IConnector c2)
    {
        IConnector? c1 = _caller;

        var result = TryConnect(c2)!;

        if (!result.Successful)
        {
            return result;
        }

        var connectors = Sort(c1, c2);

        IInput input = connectors.Input!;
        IOutput output = connectors.Output!;

        string connectionType = input.Type!;
        var connection = IConnection.NewBuilder().WithType(connectionType).Build();

        connection.Sender = output;
        connection.Receiver = input;

        input.Parent!.Parent!.Connections.Add(connection);

        return result;
    }

    public IConnectionResult? TryConnect(IConnector c2)
    {
        IConnector? c1 = _caller;

        var result = IConnectionResult.NewInstance();
        result.Successful = true;

        if (c1 == null || c2 == null)
        {
            result.Successful = false;
            result.Message = "cannot establish connection between 'null' connectors";
            return result;
        }

        if (c1.Parent == null || c2.Parent == null)
        {
            result.Successful = false;
            result.Message = "cannot establish connection between connectors without parent node";
            return result;
        }

        // Java repeats the check above verbatim here; kept so the two read alike.
        if (c1.Parent == null || c2.Parent == null)
        {
            result.Successful = false;
            result.Message = "cannot establish connection between connectors without parent node";
            return result;
        }

        if (c1.Parent.Parent == null || c2.Parent.Parent == null)
        {
            result.Successful = false;
            result.Message = "cannot establish connection between nodes that don't belong to a flow object";
            return result;
        }

        result.Successful = true;

        IInput? input = null;
        IOutput? output = null;

        var connectors = Sort(c1, c2);
        input = connectors.Input;
        output = connectors.Output;

        if (input == null || output == null)
        {
            result.Successful = false;
            result.Message = "cannot establish a connection between two outputs or two inputs";
            return result;
        }

        if (result.Successful && !Equals(c1.Type, c2.Type))
        {
            result.Successful = false;
            result.Message = "cannot establish a connection between connectors of incompatible types "
                + "[ input-type: " + input.Type + ", output-type: " + output.Type + "]";
            return result;
        }

        return result;
    }
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
