// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.json.vmfmodel.JsonModel
//
// The `new` keywords are C#: narrowing a property on redeclaration hides the base member
// rather than overriding it, so the compiler asks for the intent to be stated.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.Json.VmfModel;

interface JSONModel
{
    Json? Root { get; set; }
}

[Immutable]
interface CodeRange
{
    CodeLocation? Start { get; }
    CodeLocation? Stop { get; }
    int Length { get; }
}

[Immutable]
interface CodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[InterfaceOnly]
interface CodeElement
{
    [IgnoreToString]
    [IgnoreEquals]
    CodeRange? CodeRange { get; set; }

    [IgnoreEquals]
    CodeElement? Parent { get; set; }

    [IgnoreEquals]
    object? Payload { get; set; }
}

interface Json : CodeElement
{
    [PropertyOrder(0)] Val? Value { get; set; }
}

interface Obj : CodeElement
{
    [PropertyOrder(0)] Pair[] Pairs { get; }
}

interface Pair : CodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Key { get; set; }

    [PropertyOrder(1)] Val? Value { get; set; }
}

interface Array : CodeElement
{
    [PropertyOrder(0)] Val[] Values { get; }
}

[InterfaceOnly]
interface Val : CodeElement
{
    [GetterOnly]
    [PropertyOrder(0)] object? Value { get; }
}

interface StringValue : Val
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new string? Value { get; set; }
}

interface NumberValue : Val
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new double? Value { get; set; }
}

interface ObjectValue : Val
{
    [PropertyOrder(0)] new Obj? Value { get; set; }
}

interface ArrayValue : Val
{
    [PropertyOrder(0)] new Array? Value { get; set; }
}

interface BooleanValue : Val
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new bool? Value { get; set; }
}

interface NullValue : Val
{
    [PropertyOrder(0)] new object? Value { get; set; }
}
