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
interface WithVarName { string? VarName { get; set; } }

[InterfaceOnly]
interface WithFunctionName { string? FunctionName { get; set; } }

[InterfaceOnly]
interface WithArraySizes { string[] ArraySizes { get; } }

[InterfaceOnly]
interface CodeElement
{
    [IgnoreEquals] CodeRange? CodeRange { get; set; }
    [IgnoreEquals] CodeElement? Parent { get; set; }
    [IgnoreEquals] object? Payload { get; set; }
}

[InterfaceOnly]
interface WithId : CodeElement { int Id { get; set; } }

[InterfaceOnly]
interface ControlFlowChildNode
{
    [DelegateTo(typeof(ControlFlowChildNodeDelegate))]
    VList<ControlFlowScope>? ParentScopes();
}

[InterfaceOnly]
interface ControlFlowScope : WithId, ControlFlowChildNode
{
    Statement[] Statements { get; }
}

[InterfaceOnly]
interface ControlFlowContainer : WithId { }

[InterfaceOnly]
interface DeclStatement : WithVarName
{
    Type? DeclType { get; set; }
    new string? VarName { get; set; }
    string[] ArraySizes { get; }
}

[InterfaceOnly]
interface ConstExpression
{
    [GetterOnly] object? Value { get; }
}

[InterfaceOnly]
interface BinaryOperator : Expression
{
    Expression? Left { get; set; }
    Expression? Right { get; set; }
}

// --- code positions -----------------------------------------------------------

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

// --- top level ----------------------------------------------------------------

interface MiniClangModel { Program? Root { get; set; } }

interface Program : CodeElement
{
    [PropertyOrder(0)] PersistentComment[] Header { get; }
    [PropertyOrder(1)] Include[] Includes { get; }
    [PropertyOrder(2)] ConstantDef[] Constants { get; }
    [PropertyOrder(3)] MainFunctionDecl? MainFunction { get; set; }
    [PropertyOrder(4)] PersistentComment[] Footer { get; }
    [PropertyOrder(5)] ForwardDecl[] ForwardDeclarations { get; }
    [PropertyOrder(6)] FunctionDecl[] Functions { get; }
}

interface Include : CodeElement
{
    [PropertyOrder(0)] PersistentComment[] Comments { get; }
    [PropertyOrder(1)] string? FileName { get; set; }
}

interface ConstantDef : CodeElement, WithVarName, DeclStatement
{
    [PropertyOrder(0)] PersistentComment[] Comments { get; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [VmfDefaultValue("null")]
    [PropertyOrder(2)] int? Value { get; set; }
}

interface MainFunctionDecl : CodeElement, WithFunctionName, ControlFlowScope
{
    [PropertyOrder(0)] PersistentComment[] Comments { get; }
    [PropertyOrder(1)] new Statement[] Statements { get; }
}

interface ForwardDecl : CodeElement, WithFunctionName
{
    [PropertyOrder(0)] PersistentComment[] Comments { get; }
    [PropertyOrder(1)] Type? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] Parameter[] Params { get; }
}

interface FunctionDecl : CodeElement, WithFunctionName, ControlFlowScope
{
    [PropertyOrder(0)] PersistentComment[] Comments { get; }
    [PropertyOrder(1)] Type? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] new Statement[] Statements { get; }
    [PropertyOrder(4)] Parameter[] Params { get; }
}

// --- statements ---------------------------------------------------------------

[InterfaceOnly]
interface Statement : CodeElement, WithId, ControlFlowChildNode { }

interface BlockStatement : Statement, ControlFlowScope
{
    [PropertyOrder(0)] new Statement[] Statements { get; }
}

interface IfElseStatement : Statement, ControlFlowContainer
{
    [PropertyOrder(0)] Expression? Condition { get; set; }
    [PropertyOrder(1)] Statement? IfBlock { get; set; }
    [PropertyOrder(2)] Statement? ElseBlock { get; set; }
}

interface WhileStatement : Statement, ControlFlowContainer
{
    [PropertyOrder(0)] Expression? Check { get; set; }
    [PropertyOrder(1)] Statement? Block { get; set; }
}

interface ForStatement : Statement, ControlFlowContainer
{
    [PropertyOrder(0)] Expression? Init { get; set; }
    [PropertyOrder(1)] Expression? Check { get; set; }
    [PropertyOrder(2)] Expression? Inc { get; set; }
    [PropertyOrder(3)] Statement? Block { get; set; }
}

interface PrintStatement : Statement
{
    [PropertyOrder(0)] Expression? PrintExpression { get; set; }
    [PropertyOrder(1)] Expression[] ValueExpressions { get; }
}

interface ArrayDeclStatement : Statement, WithVarName, DeclStatement, WithArraySizes
{
    [PropertyOrder(0)] new Type? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] new string[] ArraySizes { get; }
}

interface VariableAssignmentStatement : Statement, WithVarName, DeclStatement
{
    [PropertyOrder(0)] new Type? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] Expression? AssignmentExpression { get; set; }
}

interface VarDeclStatement : Statement, WithVarName, DeclStatement
{
    [PropertyOrder(0)] new Type? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
}

interface ArrayAssignmentStatement : Statement, WithVarName, DeclStatement
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] Expression? AssignmentExpression { get; set; }
    [PropertyOrder(2)] Expression[] ArrayIndices { get; }
}

interface ReturnStatement : Statement
{
    [PropertyOrder(0)] Expression? ReturnValue { get; set; }
}

interface FunctionCallStatement : Statement, WithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] Expression[] Args { get; }
}

interface CommentStatement : Statement
{
    [PropertyOrder(0)] PersistentComment? Comment { get; set; }
}

// --- expressions --------------------------------------------------------------

[InterfaceOnly]
interface Expression : CodeElement, WithId, ControlFlowChildNode { }

interface ArrayAccessExpression : Expression
{
    [PropertyOrder(0)] Expression? ArrayVariableExpression { get; set; }
    [PropertyOrder(1)] Expression[] ArrayIndices { get; }
}

interface FunctionCallExpression : Expression, WithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] Expression[] Args { get; }
}

interface NotExpression : Expression
{
    [PropertyOrder(0)] Expression? OperatorExpression { get; set; }
}

interface AddressOperator : Expression
{
    [PropertyOrder(0)] Expression? OperatorExpression { get; set; }
}

interface DereferenceOperator : Expression
{
    [PropertyOrder(0)] Expression? OperatorExpression { get; set; }
}

interface CastOperatorExpression : Expression
{
    [PropertyOrder(0)] Type? CastType { get; set; }
    [PropertyOrder(1)] Expression? OperatorExpression { get; set; }
}

interface MultExpression : Expression
{
    [PropertyOrder(0)] Expression? Left { get; set; }
    [PropertyOrder(1)] Expression? Right { get; set; }
}

interface DivExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface AddExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface SubExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface LtExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface AndExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface EqualExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface NonEqualExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface LtEqualExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface GtEqualExpression : Expression, BinaryOperator
{
    [PropertyOrder(0)] new Expression? Left { get; set; }
    [PropertyOrder(1)] new Expression? Right { get; set; }
}

interface AssignmentExpression : Expression, DeclStatement, WithVarName
{
    [PropertyOrder(0)] new Type? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] Expression? Assignment { get; set; }
}

interface AssignmentPlusExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] Expression? Assignment { get; set; }
}

interface AssignmentMinusExpression : Expression
{
    [PropertyOrder(0)] string? VarName { get; set; }
    [PropertyOrder(1)] Expression? Assignment { get; set; }
}

interface IncPostExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface DecPostExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IncPreExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface DecPreExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IdentifierExpression : Expression, WithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

interface IntExpression : Expression, ConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new int? Value { get; set; }
}

interface DoubleExpression : Expression, ConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new double? Value { get; set; }
}

interface BooleanExpression : Expression, ConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new bool? Value { get; set; }
}

interface StringExpression : Expression, ConstExpression
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] new string? Value { get; set; }
}

interface ParenExpression : Expression
{
    [PropertyOrder(0)] Expression? ParanExpr { get; set; }
}

// --- leaves -------------------------------------------------------------------

interface Parameter : CodeElement, WithVarName, WithArraySizes, WithId, DeclStatement
{
    [PropertyOrder(0)] new Type? DeclType { get; set; }
    [PropertyOrder(1)] string? Pointer { get; set; }
    [PropertyOrder(2)] new string? VarName { get; set; }
    [PropertyOrder(3)] new string[] ArraySizes { get; }
}

interface Type : CodeElement
{
    [PropertyOrder(0)] string? TypeName { get; set; }
}

interface PersistentComment : CodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Text { get; set; }
}

interface IntLiteral : CodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] int? Value { get; set; }
}
