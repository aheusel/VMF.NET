// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.json.vmfmodel.JsonModel
//
// DEVIATION: Java narrows Val.getValue() in every subtype (String / Double / Obj / Array /
// Boolean / Object). C# interfaces cannot override a property type covariantly, and the
// generator emits a single implementation, so the subtypes inherit Value from IVal
// unchanged. The discriminated-union shape (Val + one interface per JSON value kind) is
// preserved, which is what the model is for.

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
}

[VmfModel]
public partial interface INumberValue : IVal
{
}

[VmfModel]
public partial interface IObjectValue : IVal
{
}

[VmfModel]
public partial interface IArrayValue : IVal
{
}

[VmfModel]
public partial interface IBooleanValue : IVal
{
}

[VmfModel]
public partial interface INullValue : IVal
{
}
