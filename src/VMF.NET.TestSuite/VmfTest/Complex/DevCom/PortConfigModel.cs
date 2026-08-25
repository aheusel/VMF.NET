// Ported from eu.mihosoft.vmftest.complex.devcom.vmfmodel.PortConfig
//
// DEVIATIONS:
//  - Java declares StopBits/ParityBits/State as @ExternalType stand-ins for enums declared
//    outside the model. In C# they are real enums, referenced directly.
//  - PortConfig/PortInfo redeclare Name (and ExtendedName) to attach defaults; C# needs
//    `new` for that.

using System;
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.DevCom.VmfModel;




[InterfaceOnly]
interface IWithName
{
    [Doc("The port name used to identify the port, e.g. 'COM3'.")]
    [VmfDefaultValue("\"COM0\"")]
    [GetterOnly]
    string? Name { get; }
}

[InterfaceOnly]
interface IWithExtendedName
{
    [Doc("The extended port name, e.g., 'COM3 - Arduino UNO'")]
    [VmfDefaultValue("\"\"")]
    [GetterOnly]
    string? ExtendedName { get; }
}

[Doc("COM port configuration used to configure a physical or virtual COM port.")]
[Immutable]
interface IPortConfig : IWithName
{
    [Doc("The number of data bits (usually 8).")]
    [VmfDefaultValue("8")]
    int NumberOfDataBits { get; }

    [Doc("The baud rate used for sending and receiving data.")]
    [VmfDefaultValue("115200")]
    int BaudRate { get; }

    [Doc("The number of parity bits.")]
    [VmfDefaultValue("ParityBits.NoParity")]
    ParityBits ParityBits { get; }

    [Doc("The number of stop bits.")]
    [VmfDefaultValue("StopBits.OneStopBit")]
    StopBits StopBits { get; }

    [Doc("Determines, whether RS485 mode should be enabled")]
    [VmfDefaultValue("false")]
    bool IsRS485ModeEnabled { get; }

    [Doc("Safety timeout used for opening the port (in milliseconds).")]
    [VmfDefaultValue("200")]
    int SafetyTimeout { get; }

    [Doc("Write timeout (in milliseconds).")]
    [VmfDefaultValue("0")]
    int WriteTimeout { get; }
}

[Immutable]
[VmfEquals]
interface IPortInfo : IWithName, IWithExtendedName
{
    [Doc("The port description. Some devices add the serial number (e.g. FTDI chips).")]
    [VmfDefaultValue("\"\"")]
    [IgnoreEquals]
    string? Description { get; }

    [Doc("The port location.")]
    [VmfDefaultValue("\"\"")]
    [IgnoreEquals]
    string? Location { get; }
}

[Doc("Denotes a device accessed with this library")]
[Immutable]
interface IDeviceInfo
{
    [Doc("Returns the device class")]
    string? DeviceClass { get; }

    [Doc("Returns the device")]
    string? Device { get; }

    [Doc("Returns the MCU type used by this device")]
    string? MCUType { get; }

    [Doc("Returns the serial number of the device")]
    string? SerialNumber { get; }
}

[Doc("Port event.")]
[Immutable]
interface IPortEvent
{
    [Doc("Timestamp (milliseconds since January 1st, 1970).")]
    long Timestamp { get; }

    [Doc("port infos of ports added since the last scan.")]
    VList<IPortInfo> Added { get; }

    [Doc("port infos of ports removed since the last scan.")]
    VList<IPortInfo> Removed { get; }
}

[Doc("State changed event.")]
[Immutable]
interface IStateChangedEvent
{
    [Doc("Timestamp (milliseconds since January 1st, 1970).")]
    long Timestamp { get; }

    [Doc("Old state")]
    State OldState { get; }

    [Doc("New state")]
    State NewState { get; }

    Exception? Exception { get; }
}
