// Ported from the four behaviour delegates beside eu.mihosoft.vmftest.complex.vflow.vmfmodel.VFlow.
// They live next to the generated API, as Java's do in the package VMF generates into.

using System.Linq;
using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.Complex.VFlow;

// --- behavior delegates ---

public sealed class ConnectorDelegate : IDelegatedBehavior<Connector>
{
    private Connector? _caller;
    public void SetCaller(Connector caller) => _caller = caller;

    public void OnConnectorInstantiated()
    {
        // prevent duplicates & set id
        _caller!.Connections.AddChangeListener(evt =>
        {
            foreach (Connection cnn in evt.Added.Cast<Connection>())
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
        public Input? Input;
        public Output? Output;

        public ConnectorTuple(Input? input, Output? output)
        {
            Input = input;
            Output = output;
        }
    }

    private ConnectorTuple Sort(Connector? c1, Connector? c2)
    {
        Input? input = null;
        Output? output = null;

        if (c1 is Input && c2 is Output)
        {
            input = (Input)c1;
            output = (Output)c2;
        }
        else if (c1 is Output && c2 is Input)
        {
            input = (Input)c2;
            output = (Output)c1;
        }

        return new ConnectorTuple(input, output);
    }

    public ConnectionResult? Connect(Connector c2)
    {
        Connector? c1 = _caller;

        var result = TryConnect(c2)!;

        if (!result.Successful)
        {
            return result;
        }

        var connectors = Sort(c1, c2);

        Input input = connectors.Input!;
        Output output = connectors.Output!;

        string connectionType = input.Type!;
        var connection = Connection.NewBuilder().WithType(connectionType).Build();

        connection.Sender = output;
        connection.Receiver = input;

        input.Parent!.Parent!.Connections.Add(connection);

        return result;
    }

    public ConnectionResult? TryConnect(Connector c2)
    {
        Connector? c1 = _caller;

        var result = ConnectionResult.NewInstance();
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

        Input? input = null;
        Output? output = null;

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

public sealed class ConnectionDelegate : IDelegatedBehavior<Connection>
{
    private Connection? _caller;
    public void SetCaller(Connection caller) => _caller = caller;

    public void OnConnectionInstantiated()
    {
        // Java's changePermitted flag is omitted: the only code that reads it -- a guard
        // rejecting manual writes to 'id' -- is commented out there.
        _caller!.VMF.Reflect.PropertyByName("Sender")?.AddChangeListener(_ => SyncId());
        _caller!.VMF.Reflect.PropertyByName("Receiver")?.AddChangeListener(_ => SyncId());
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

public sealed class VNodeDelegate : IDelegatedBehavior<VNode>
{
    private VNode? _caller;
    public void SetCaller(VNode caller) => _caller = caller;

    public Input? AddInput(string type)
    {
        var input = Input.NewBuilder().WithType(type).Build();
        _caller!.Inputs.Add(input);
        return input;
    }

    public Output? AddOutput(string type)
    {
        var outputs = Output.NewBuilder().WithType(type).Build();
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
            foreach (VNode n in evt.Added.Cast<VNode>())
            {
                if (_caller.Nodes.Count(m => ReferenceEquals(n, m)) > 1)
                {
                    throw new System.InvalidOperationException("Duplicate nodes added: " + n);
                }
            }
        });
    }

    public ConnectionResult? Connect(Connector c1, Connector c2) => null;
    public ConnectionResult? TryConnect(Connector c1, Connector c2) => null;
    public ConnectionResult? Connect(VNode n1, VNode n2, string type) => null;

    public VNode? NewNode(object o)
    {
        var n = VNode.NewInstance();
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
