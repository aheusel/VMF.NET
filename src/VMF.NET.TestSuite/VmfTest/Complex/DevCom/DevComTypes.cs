// The enums the PortConfig model references. Java declares @ExternalType stand-ins for these
// because they live outside its model package; C# references the real enums directly.

namespace VMF.NET.TestSuite.VmfTest.Complex.DevCom;

public enum StopBits { OneStopBit, OnePointFiveStopBits, TwoStopBits }
public enum ParityBits { NoParity, OddParity, EvenParity, MarkParity, SpaceParity }
public enum State { Disconnected, Connecting, Connected, Error }
