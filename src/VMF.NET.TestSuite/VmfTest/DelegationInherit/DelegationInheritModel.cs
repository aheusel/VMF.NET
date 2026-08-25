// Ported from eu.mihosoft.vmftest.delegationinherit.vmfmodel.DelegationInherit,
// DeviceDelegate and CircuitDeviceDelegate.
//
// A compile-only model, as in Java: it has no test class. What it pins is that ICircuitDevice's
// Process()/Consume() take their delegate from the TYPE-level [DelegateTo] -- the Java source
// comments those two lines "uses constructor delegation info" -- while Produce() names it itself.
//
// The `new` keywords are C#: re-declaring an inherited interface method otherwise warns CS0108.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.DelegationInherit.VmfModel;

[InterfaceOnly]
interface IProducer
{
    void Produce();
}

[InterfaceOnly]
interface IConsumer
{
    void Consume();
}

[InterfaceOnly]
interface IProcessor : IProducer, IConsumer
{
    void Process();
}

interface IDevice : IProcessor
{
    [DelegateTo(typeof(DeviceDelegate))]
    new void Process();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Consume();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Produce();
}

[DelegateTo(typeof(CircuitDeviceDelegate))]
interface ICircuitDevice : IDevice
{
    // uses constructor delegation info
    new void Process();

    // uses constructor delegation info
    new void Consume();

    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Produce();
}
