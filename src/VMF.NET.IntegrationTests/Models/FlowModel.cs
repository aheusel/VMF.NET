// Model interfaces for integration testing.
// Models a simple flow graph: Flow contains Nodes, Nodes have Connections.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.IntegrationTests.Models;

/// <summary>
/// A flow graph containing nodes and connections.
/// </summary>
[VmfModel(Equality = EqualsType.All)]
public partial interface IFlow
{
    string? Title { get; set; }

    [Contains("INode.Flow")]
    VList<INode> Nodes { get; }

    [Contains("IConnection.Flow")]
    VList<IConnection> Connections { get; }
}

/// <summary>
/// A node in a flow graph.
/// </summary>
public partial interface INode
{
    string? Name { get; set; }

    int X { get; set; }

    int Y { get; set; }

    [Container("IFlow.Nodes")]
    IFlow? Flow { get; }

    [Refers("IConnection.Sender")]
    VList<IConnection> Outputs { get; }

    [Refers("IConnection.Receiver")]
    VList<IConnection> Inputs { get; }
}

/// <summary>
/// A connection between two nodes.
/// </summary>
public partial interface IConnection
{
    [Container("IFlow.Connections")]
    IFlow? Flow { get; }

    [Refers("INode.Outputs")]
    INode? Sender { get; set; }

    [Refers("INode.Inputs")]
    INode? Receiver { get; set; }
}
