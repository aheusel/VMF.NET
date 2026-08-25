// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.json.vmfmodel.JsonModel
//
// The `new` keywords are C#: narrowing a property on redeclaration hides the base member
// rather than overriding it, so the compiler asks for the intent to be stated.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.Json.VmfModel;

interface IJSONModel
{
    IJson? Root { get; set; }
}

[Immutable]
interface ICodeRange
{
    ICodeLocation? Start { get; }
    ICodeLocation? Stop { get; }
    int Length { get; }
}

[Immutable]
interface ICodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[InterfaceOnly]
interface ICodeElement
{
    [IgnoreToString]
    [IgnoreEquals]
    ICodeRange? CodeRange { get; set; }

    [IgnoreEquals]
    ICodeElement? Parent { get; set; }

    [IgnoreEquals]
    object? Payload { get; set; }
}

interface IJson : ICodeElement
{
    [PropertyOrder(0)] IVal? Value { get; set; }
}

interface IObj : ICodeElement
{
    [PropertyOrder(0)] IPair[] Pairs { get; }
}

interface IPair : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Key { get; set; }

    [PropertyOrder(1)] IVal? Value { get; set; }
}

interface IArray : ICodeElement
{
    [PropertyOrder(0)] IVal[] Values { get; }
}

[InterfaceOnly]
interface IVal : ICodeElement
{
    [GetterOnly]
    [PropertyOrder(0)] object? Value { get; }
}

interface IStringValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new string? Value { get; set; }
}

interface INumberValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new double? Value { get; set; }
}

interface IObjectValue : IVal
{
    [PropertyOrder(0)] new IObj? Value { get; set; }
}

interface IArrayValue : IVal
{
    [PropertyOrder(0)] new IArray? Value { get; set; }
}

interface IBooleanValue : IVal
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new bool? Value { get; set; }
}

interface INullValue : IVal
{
    [PropertyOrder(0)] new object? Value { get; set; }
}
