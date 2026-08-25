// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.miniclang.vmfmodel.MiniClangModel
//
// DEVIATIONS:
//  1. Members inherited from two unrelated mixins (ArraySizes, VarName, Statements, Left,
//     Right, FunctionName, DeclType) are re-declared with `new` to resolve CS0229.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.MiniCLang.VmfModel;

// --- mixins -------------------------------------------------------------------

[InterfaceOnly]
interface IWithVarName { string? VarName { get; set; } }

[InterfaceOnly]
interface IWithFunctionName { string? FunctionName { get; set; } }

[InterfaceOnly]
interface IWithArraySizes { string[] ArraySizes { get; } }

[InterfaceOnly]
interface ICodeElement
{
    [IgnoreEquals] ICodeRange? CodeRange { get; set; }
    [IgnoreEquals] ICodeElement? Parent { get; set; }
    [IgnoreEquals] object? Payload { get; set; }
}

[InterfaceOnly]
interface IWithId : ICodeElement { int Id { get; set; } }

[InterfaceOnly]
interface IControlFlowChildNode
{
    [DelegateTo(typeof(ControlFlowChildNodeDelegate))]
    VList<IControlFlowScope>? ParentScopes();
}

[InterfaceOnly]
interface IControlFlowScope : IWithId, IControlFlowChildNode
{
    IStatement[] Statements { get; }
}

[InterfaceOnly]
interface IControlFlowContainer : IWithId { }

[InterfaceOnly]
interface IDeclStatement : IWithVarName
{
    IType? DeclType { get; set; }
    new string? VarName { get; set; }
    string[] ArraySizes { get; }
}

[InterfaceOnly]
interface IConstExpression
{
    [GetterOnly] object? Value { get; }
}

[InterfaceOnly]
interface IBinaryOperator : IExpression
{
    IExpression? Left { get; set; }
    IExpression? Right { get; set; }
}

// --- code positions -----------------------------------------------------------

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

// --- top level ----------------------------------------------------------------

interface IMiniClangModel { IProgram? Root { get; set; } }

interface IProgram : ICodeElement
{
    [PropertyOrder(0)] IPersistentComment[] Header { get; }
    [PropertyOrder(1)] IInclude[] Includes { get; }
    [PropertyOrder(2)] IConstantDef[] Constants { get; }
    [PropertyOrder(3)] IMainFunctionDecl? MainFunction { get; set; }
    [PropertyOrder(4)] IPersistentComment[] Footer { get; }
    [PropertyOrder(5)] IForwardDecl[] ForwardDeclarations { get; }
    [PropertyOrder(6)] IFunctionDecl[] Functions { get; }
}

interface IInclude : ICodeElement
{
    [PropertyOrder(0)] IPersistentComment[] Comments { get; }
    [PropertyOrder(1)] string? FileName { get; set; }
}

interface IConstantDef : ICodeElement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] IPersistentComment[] Comments { get; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [VmfDefaultValue("null")]
    [PropertyOrder(2)] int? Value { get; set; }
}

interface IMainFunctionDecl : ICodeElement, IWithFunctionName, IControlFlowScope
{
    [PropertyOrder(0)] IPersistentComment[] Comments { get; }
    [PropertyOrder(1)] new IStatement[] Statements { get; }
}

interface IForwardDecl : ICodeElement, IWithFunctionName
{
    [PropertyOrder(0)] IPersistentComment[] Comments { get; }
    [PropertyOrder(1)] IType? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] IParameter[] Params { get; }
}

interface IFunctionDecl : ICodeElement, IWithFunctionName, IControlFlowScope
{
    [PropertyOrder(0)] IPersistentComment[] Comments { get; }
    [PropertyOrder(1)] IType? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] new IStatement[] Statements { get; }
    [PropertyOrder(4)] IParameter[] Params { get; }
}

// --- statements ---------------------------------------------------------------

[InterfaceOnly]
interface IStatement : ICodeElement, IWithId, IControlFlowChildNode { }

interface IBlockStatement : IStatement, IControlFlowScope
{
    [PropertyOrder(0)] new IStatement[] Statements { get; }
}

interface IIfElseStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Condition { get; set; }
    [PropertyOrder(1)] IStatement? IfBlock { get; set; }
    [PropertyOrder(2)] IStatement? ElseBlock { get; set; }
}

interface IWhileStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Check { get; set; }
    [PropertyOrder(1)] IStatement? Block { get; set; }
}

interface IForStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Init { get; set; }
    [PropertyOrder(1)] IExpression? Check { get; set; }
    [PropertyOrder(2)] IExpression? Inc { get; set; }
    [PropertyOrder(3)] IStatement? Block { get; set; }
}

interface IPrintStatement : IStatement
{
    [PropertyOrder(0)] IExpression? PrintExpression { get; set; }
    [PropertyOrder(1)] IExpression[] ValueExpressions { get; }
}

interface IArrayDeclStatement : IStatement, IWithVarName, IDeclStatement, IWithArraySizes
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] new string[] ArraySizes { get; }
}

interface IVariableAssignmentStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] IExpression? AssignmentExpression { get; set; }
}

interface IVarDeclStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
}

interface IArrayAssignmentStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? AssignmentExpression { get; set; }
    [PropertyOrder(2)] IExpression[] ArrayIndices { get; }
}

interface IReturnStatement : IStatement
{
    [PropertyOrder(0)] IExpression? ReturnValue { get; set; }
}

interface IFunctionCallStatement : IStatement, IWithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] IExpression[] Args { get; }
}

interface ICommentStatement : IStatement
{
    [PropertyOrder(0)] IPersistentComment? Comment { get; set; }
}

// --- expressions --------------------------------------------------------------

[InterfaceOnly]
interface IExpression : ICodeElement, IWithId, IControlFlowChildNode { }

interface IArrayAccessExpression : IExpression
{
    [PropertyOrder(0)] IExpression? ArrayVariableExpression { get; set; }
    [PropertyOrder(1)] IExpression[] ArrayIndices { get; }
}

interface IFunctionCallExpression : IExpression, IWithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] IExpression[] Args { get; }
}

interface INotExpression : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

interface IAddressOperator : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

interface IDereferenceOperator : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

interface ICastOperatorExpression : IExpression
{
    [PropertyOrder(0)] IType? CastType { get; set; }
    [PropertyOrder(1)] IExpression? OperatorExpression { get; set; }
}

interface IMultExpression : IExpression
{
    [PropertyOrder(0)] IExpression? Left { get; set; }
    [PropertyOrder(1)] IExpression? Right { get; set; }
}

interface IDivExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface IAddExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface ISubExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface ILtExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface IAndExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface IEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface INonEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface ILtEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface IGtEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

interface IAssignmentExpression : IExpression, IDeclStatement, IWithVarName
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] IExpression? Assignment { get; set; }
}

interface IAssignmentPlusExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? Assignment { get; set; }
}

interface IAssignmentMinusExpression : IExpression
{
    [PropertyOrder(0)] string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? Assignment { get; set; }
}

interface IIncPostExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IDecPostExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IIncPreExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IDecPreExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IIdentifierExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IIntExpression : IExpression, IConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new int? Value { get; set; }
}

interface IDoubleExpression : IExpression, IConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new double? Value { get; set; }
}

interface IBooleanExpression : IExpression, IConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new bool? Value { get; set; }
}

interface IStringExpression : IExpression, IConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new string? Value { get; set; }
}

interface IParenExpression : IExpression
{
    [PropertyOrder(0)] IExpression? ParanExpr { get; set; }
}

// --- leaves -------------------------------------------------------------------

interface IParameter : ICodeElement, IWithVarName, IWithArraySizes, IWithId, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] string? Pointer { get; set; }
    [PropertyOrder(2)] new string? VarName { get; set; }
    [PropertyOrder(3)] new string[] ArraySizes { get; }
}

interface IType : ICodeElement
{
    [PropertyOrder(0)] string? TypeName { get; set; }
}

interface IPersistentComment : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Text { get; set; }
}

interface IIntLiteral : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] int? Value { get; set; }
}
