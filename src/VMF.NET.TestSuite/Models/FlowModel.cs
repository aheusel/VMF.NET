// Model interfaces for integration testing.
// Models a simple flow graph: Flow contains Nodes, Nodes have Connections.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.Models.VmfModel;

/// <summary>
/// A flow graph containing nodes and connections.
/// </summary>
[VmfModel(Equality = EqualsType.All)]
interface Flow
{
    string? Title { get; set; }

    [Contains("Node.Flow")]
    Node[] Nodes { get; }

    [Contains("Connection.Flow")]
    Connection[] Connections { get; }
}

/// <summary>
/// A node in a flow graph.
/// </summary>
interface Node
{
    string? Name { get; set; }

    int X { get; set; }

    int Y { get; set; }

    [Container("Flow.Nodes")]
    Flow? Flow { get; }

    [Refers("Connection.Sender")]
    Connection[] Outputs { get; }

    [Refers("Connection.Receiver")]
    Connection[] Inputs { get; }
}

/// <summary>
/// A connection between two nodes.
/// </summary>
interface Connection
{
    [Container("Flow.Connections")]
    Flow? Flow { get; }

    [Refers("Node.Outputs")]
    Node? Sender { get; set; }

    [Refers("Node.Inputs")]
    Node? Receiver { get; set; }
}
