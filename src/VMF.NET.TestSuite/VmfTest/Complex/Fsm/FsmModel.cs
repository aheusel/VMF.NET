// Ported from eu.mihosoft.vmftest.complex.fsm.vmfmodel.FSM

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.Fsm.VmfModel;

[Doc("This model entity is a finite state machine.")]
[VmfEquals]
interface FSM
{
    string? Name { get; set; }
    State? InitialState { get; set; }
    State? CurrentState { get; set; }
    State[] FinalState { get; }

    [Contains("State.OwningFSM")]
    State[] OwnedState { get; }
}

[VmfEquals]
interface State
{
    string? Name { get; set; }

    [Container("FSM.OwnedState")]
    FSM? OwningFSM { get; }

    [Contains("Transition.Source")]
    Transition[] OutgoingTransitions { get; }

    [Contains("Transition.Target")]
    Transition[] IncomingTransitions { get; }
}

[VmfEquals]
interface Transition
{
    string? Input { get; set; }
    string? Output { get; set; }

    [Container("State.OutgoingTransitions")]
    State? Source { get; }

    [Container("State.IncomingTransitions")]
    State? Target { get; }

    IAction[] Actions { get; }
}

[VmfEquals]
interface IAction
{
    string? Name { get; set; }
}
