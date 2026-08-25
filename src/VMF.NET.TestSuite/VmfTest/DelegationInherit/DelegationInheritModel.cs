// Ported from eu.mihosoft.vmftest.delegationinherit.vmfmodel.DelegationInherit,
// DeviceDelegate and CircuitDeviceDelegate.
//
// A compile-only model, as in Java: it has no test class. What it pins is that CircuitDevice's
// Process()/Consume() take their delegate from the TYPE-level [DelegateTo] -- the Java source
// comments those two lines "uses constructor delegation info" -- while Produce() names it itself.
//
// The `new` keywords are C#: re-declaring an inherited interface method otherwise warns CS0108.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.DelegationInherit.VmfModel;

[InterfaceOnly]
interface Producer
{
    void Produce();
}

[InterfaceOnly]
interface Consumer
{
    void Consume();
}

[InterfaceOnly]
interface Processor : Producer, Consumer
{
    void Process();
}

interface Device : Processor
{
    [DelegateTo(typeof(DeviceDelegate))]
    new void Process();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Consume();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Produce();
}

[DelegateTo(typeof(CircuitDeviceDelegate))]
interface CircuitDevice : Device
{
    // uses constructor delegation info
    new void Process();

    // uses constructor delegation info
    new void Consume();

    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Produce();
}
