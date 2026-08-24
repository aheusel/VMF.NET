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

namespace VMF.NET.TestSuite.VmfTest.DelegationInherit;

[VmfModel]
[InterfaceOnly]
public partial interface IProducer
{
    void Produce();
}

[VmfModel]
[InterfaceOnly]
public partial interface IConsumer
{
    void Consume();
}

[VmfModel]
[InterfaceOnly]
public partial interface IProcessor : IProducer, IConsumer
{
    void Process();
}

[VmfModel]
public partial interface IDevice : IProcessor
{
    [DelegateTo(typeof(DeviceDelegate))]
    new void Process();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Consume();

    [DelegateTo(typeof(DeviceDelegate))]
    new void Produce();
}

[VmfModel]
[DelegateTo(typeof(CircuitDeviceDelegate))]
public partial interface ICircuitDevice : IDevice
{
    // uses constructor delegation info
    new void Process();

    // uses constructor delegation info
    new void Consume();

    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Produce();
}

public sealed class DeviceDelegate : IDelegatedBehavior<IDevice>
{
    public void Consume() { }
    public void Produce() { }
    public void Process() { }
}

public sealed class CircuitDeviceDelegate : IDelegatedBehavior<IDevice>
{
    public void OnCircuitDeviceInstantiated()
    {
    }

    public void Consume() { }
    public void Produce() { }
    public void Process() { }
}
