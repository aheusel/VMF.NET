// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.json.vmfmodel.JsonModel
//
// The `new` keywords are C#: narrowing a property on redeclaration hides the base member
// rather than overriding it, so the compiler asks for the intent to be stated.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.Json;

[VmfModel]
public partial interface IJSONModel
{
    IJson? Root { get; set; }
}

[VmfModel]
[Immutable]
public partial interface ICodeRange
{
    ICodeLocation? Start { get; }
    ICodeLocation? Stop { get; }
    int Length { get; }
}

[VmfModel]
[Immutable]
public partial interface ICodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface ICodeElement
{
    [IgnoreToString]
    [IgnoreEquals]
    ICodeRange? CodeRange { get; set; }

    [IgnoreEquals]
    ICodeElement? Parent { get; set; }

    [IgnoreEquals]
    object? Payload { get; set; }
}

[VmfModel]
public partial interface IJson : ICodeElement
{
    [PropertyOrder(0)] IVal? Value { get; set; }
}

[VmfModel]
public partial interface IObj : ICodeElement
{
    [PropertyOrder(0)] VList<IPair> Pairs { get; }
}

[VmfModel]
public partial interface IPair : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Key { get; set; }

    [PropertyOrder(1)] IVal? Value { get; set; }
}

[VmfModel]
public partial interface IArray : ICodeElement
{
    [PropertyOrder(0)] VList<IVal> Values { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IVal : ICodeElement
{
    [GetterOnly]
    [PropertyOrder(0)] object? Value { get; }
}

[VmfModel]
public partial interface IStringValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new string? Value { get; set; }
}

[VmfModel]
public partial interface INumberValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new double? Value { get; set; }
}

[VmfModel]
public partial interface IObjectValue : IVal
{
    [PropertyOrder(0)] new IObj? Value { get; set; }
}

[VmfModel]
public partial interface IArrayValue : IVal
{
    [PropertyOrder(0)] new IArray? Value { get; set; }
}

[VmfModel]
public partial interface IBooleanValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new bool? Value { get; set; }
}

[VmfModel]
public partial interface INullValue : IVal
{
    [PropertyOrder(0)] new object? Value { get; set; }
}
