// Ported from eu.mihosoft.vmftest.complex.vflow.vmfmodel.VFlow
//
// DEVIATIONS (two generator gaps, see the area README notes):
//  1. Inherited [DelegateTo] methods are NOT generated in derived types -- only methods
//     declared on the type itself are. Input/Output/VFlow therefore re-declare the
//     delegated methods they inherit.
//  2. A `new` redeclaration of a property leaves the BASE read-only interface member
//     unimplemented, so Parent/Connections are declared only on Input/Output, not on
//     Connector as in Java.
//  Because a delegate is cast to IDelegatedBehavior<DeclaringType>, the shared delegates
//  implement that interface once per model type that uses them.

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

    [DelegateTo(typeof(ConnectorDelegate))]
    new IConnectionResult? TryConnect(IConnector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    new IConnectionResult? Connect(IConnector c);
}

[VmfModel]
public partial interface IOutput : IConnector
{
    [Container("IVNode.Outputs")]
    IVNode? Parent { get; }

    [Contains("IConnection.Sender")]
    VList<IConnection> Connections { get; }

    [DelegateTo(typeof(ConnectorDelegate))]
    new IConnectionResult? TryConnect(IConnector c);

    [DelegateTo(typeof(ConnectorDelegate))]
    new IConnectionResult? Connect(IConnector c);
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

    // inherited from IVNode -- re-declared because inherited delegations are not generated
    [DelegateTo(typeof(VNodeDelegate))]
    new IInput? AddInput(string type);

    [DelegateTo(typeof(VNodeDelegate))]
    new IOutput? AddOutput(string type);
}

// --- behavior delegates ---

public sealed class ConnectorDelegate
    : IDelegatedBehavior<IConnector>, IDelegatedBehavior<IInput>, IDelegatedBehavior<IOutput>
{
    private IConnector? _caller;
    void IDelegatedBehavior<IConnector>.SetCaller(IConnector caller) => _caller = caller;
    void IDelegatedBehavior<IInput>.SetCaller(IInput caller) => _caller = caller;
    void IDelegatedBehavior<IOutput>.SetCaller(IOutput caller) => _caller = caller;

    public IConnectionResult? TryConnect(IConnector c) => null;
    public IConnectionResult? Connect(IConnector c) => null;
}

public sealed class ConnectionDelegate : IDelegatedBehavior<IConnection>
{
    private IConnection? _caller;
    public void SetCaller(IConnection caller) => _caller = caller;
}

public sealed class VNodeDelegate : IDelegatedBehavior<IVNode>, IDelegatedBehavior<IVFlow>
{
    private IVNode? _caller;
    void IDelegatedBehavior<IVNode>.SetCaller(IVNode caller) => _caller = caller;
    void IDelegatedBehavior<IVFlow>.SetCaller(IVFlow caller) => _caller = caller;

    public IInput? AddInput(string type)
    {
        var input = IInput.NewInstance();
        input.Type = type;
        _caller!.Inputs.Add(input);
        return input;
    }

    public IOutput? AddOutput(string type)
    {
        var output = IOutput.NewInstance();
        output.Type = type;
        _caller!.Outputs.Add(output);
        return output;
    }
}

public sealed class VFlowDelegate : IDelegatedBehavior<IVFlow>
{
    private IVFlow? _caller;
    public void SetCaller(IVFlow caller) => _caller = caller;

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
