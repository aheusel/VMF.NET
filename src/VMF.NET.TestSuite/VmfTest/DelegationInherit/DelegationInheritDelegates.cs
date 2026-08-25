// Ported from eu.mihosoft.vmftest.delegationinherit.DeviceDelegate and CircuitDeviceDelegate.

using System.Linq;
using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.DelegationInherit;

public sealed class DeviceDelegate : IDelegatedBehavior<Device>
{
    public void Consume() { }
    public void Produce() { }
    public void Process() { }
}

public sealed class CircuitDeviceDelegate : IDelegatedBehavior<Device>
{
    public void OnCircuitDeviceInstantiated()
    {
    }

    public void Consume() { }
    public void Produce() { }
    public void Process() { }
}
