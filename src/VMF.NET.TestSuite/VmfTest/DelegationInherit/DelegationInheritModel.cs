// Ported from eu.mihosoft.vmftest.delegationinherit.vmfmodel.DelegationInherit
//
// DEVIATION: Java's CircuitDevice leaves process()/consume() without a method-level
// @DelegateTo and relies on the type-level (constructor) delegation to supply them.
// VMF.NET generates method bodies only from method-level [DelegateTo], so the two
// methods carry an explicit attribute here. The type-level [DelegateTo] is kept so the
// constructor-delegation path is still exercised.

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
    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Process();

    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Consume();

    [DelegateTo(typeof(CircuitDeviceDelegate))]
    new void Produce();
}

public sealed class DeviceDelegate : IDelegatedBehavior<IDevice>
{
    private IDevice? _caller;
    public void SetCaller(IDevice caller) => _caller = caller;

    public int ProduceCount { get; private set; }
    public int ConsumeCount { get; private set; }
    public int ProcessCount { get; private set; }

    public void Produce() => ProduceCount++;
    public void Consume() => ConsumeCount++;
    public void Process() => ProcessCount++;
}

public sealed class CircuitDeviceDelegate : IDelegatedBehavior<ICircuitDevice>
{
    private ICircuitDevice? _caller;
    public void SetCaller(ICircuitDevice caller) => _caller = caller;

    public int ProduceCount { get; private set; }
    public int ConsumeCount { get; private set; }
    public int ProcessCount { get; private set; }

    public void Produce() => ProduceCount++;
    public void Consume() => ConsumeCount++;
    public void Process() => ProcessCount++;
}
