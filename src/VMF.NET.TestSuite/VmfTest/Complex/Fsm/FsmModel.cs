// Ported from eu.mihosoft.vmftest.complex.fsm.vmfmodel.FSM

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Fsm.VmfModel;

[Doc("This model entity is a finite state machine.")]
[VmfEquals]
interface IFSM
{
    string? Name { get; set; }
    IState? InitialState { get; set; }
    IState? CurrentState { get; set; }
    IState[] FinalState { get; }

    [Contains("IState.OwningFSM")]
    IState[] OwnedState { get; }
}

[VmfEquals]
interface IState
{
    string? Name { get; set; }

    [Container("IFSM.OwnedState")]
    IFSM? OwningFSM { get; }

    [Contains("ITransition.Source")]
    ITransition[] OutgoingTransitions { get; }

    [Contains("ITransition.Target")]
    ITransition[] IncomingTransitions { get; }
}

[VmfEquals]
interface ITransition
{
    string? Input { get; set; }
    string? Output { get; set; }

    [Container("IState.OutgoingTransitions")]
    IState? Source { get; }

    [Container("IState.IncomingTransitions")]
    IState? Target { get; }

    IAction[] Actions { get; }
}

[VmfEquals]
interface IAction
{
    string? Name { get; set; }
}
