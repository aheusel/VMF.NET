// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.miniclang.vmfmodel.MiniClangModel
//
// DEVIATIONS:
//  1. Covariant property narrowing (ConstExpression.getValue() -> Integer/Double/Boolean/
//     String) has no C# equivalent on interfaces, so the const-expression types inherit
//     Value from IConstExpression unchanged.
//  2. Members inherited from two unrelated mixins (ArraySizes, VarName, Statements, Left,
//     Right, FunctionName, DeclType) are re-declared with `new` to resolve CS0229.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.MiniCLang;

// --- mixins -------------------------------------------------------------------

[VmfModel][InterfaceOnly]
public partial interface IWithVarName { string? VarName { get; set; } }

[VmfModel][InterfaceOnly]
public partial interface IWithFunctionName { string? FunctionName { get; set; } }

[VmfModel][InterfaceOnly]
public partial interface IWithArraySizes { VList<string> ArraySizes { get; } }

[VmfModel][InterfaceOnly]
public partial interface ICodeElement
{
    [IgnoreEquals] ICodeRange? CodeRange { get; set; }
    [IgnoreEquals] ICodeElement? Parent { get; set; }
    [IgnoreEquals] object? Payload { get; set; }
}

[VmfModel][InterfaceOnly]
public partial interface IWithId : ICodeElement { int Id { get; set; } }

[VmfModel][InterfaceOnly]
public partial interface IControlFlowChildNode
{
    [DelegateTo(typeof(ControlFlowChildNodeDelegate))]
    VList<IControlFlowScope>? ParentScopes();
}

[VmfModel][InterfaceOnly]
public partial interface IControlFlowScope : IWithId, IControlFlowChildNode
{
    VList<IStatement> Statements { get; }
}

[VmfModel][InterfaceOnly]
public partial interface IControlFlowContainer : IWithId { }

[VmfModel][InterfaceOnly]
public partial interface IDeclStatement : IWithVarName
{
    IType? DeclType { get; set; }
    new string? VarName { get; set; }
    VList<string> ArraySizes { get; }
}

[VmfModel][InterfaceOnly]
public partial interface IConstExpression
{
    [GetterOnly] object? Value { get; }
}

[VmfModel][InterfaceOnly]
public partial interface IBinaryOperator : IExpression
{
    IExpression? Left { get; set; }
    IExpression? Right { get; set; }
}

// --- code positions -----------------------------------------------------------

[VmfModel][Immutable]
public partial interface ICodeRange
{
    ICodeLocation? Start { get; }
    ICodeLocation? Stop { get; }
    int Length { get; }
}

[VmfModel][Immutable]
public partial interface ICodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

// --- top level ----------------------------------------------------------------

[VmfModel]
public partial interface IMiniClangModel { IProgram? Root { get; set; } }

[VmfModel]
public partial interface IProgram : ICodeElement
{
    [PropertyOrder(0)] VList<IPersistentComment> Header { get; }
    [PropertyOrder(1)] VList<IInclude> Includes { get; }
    [PropertyOrder(2)] VList<IConstantDef> Constants { get; }
    [PropertyOrder(3)] IMainFunctionDecl? MainFunction { get; set; }
    [PropertyOrder(4)] VList<IPersistentComment> Footer { get; }
    [PropertyOrder(5)] VList<IForwardDecl> ForwardDeclarations { get; }
    [PropertyOrder(6)] VList<IFunctionDecl> Functions { get; }
}

[VmfModel]
public partial interface IInclude : ICodeElement
{
    [PropertyOrder(0)] VList<IPersistentComment> Comments { get; }
    [PropertyOrder(1)] string? FileName { get; set; }
}

[VmfModel]
public partial interface IConstantDef : ICodeElement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] VList<IPersistentComment> Comments { get; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [VmfDefaultValue("null")]
    [PropertyOrder(2)] int? Value { get; set; }
}

[VmfModel]
public partial interface IMainFunctionDecl : ICodeElement, IWithFunctionName, IControlFlowScope
{
    [PropertyOrder(0)] VList<IPersistentComment> Comments { get; }
    [PropertyOrder(1)] new VList<IStatement> Statements { get; }
}

[VmfModel]
public partial interface IForwardDecl : ICodeElement, IWithFunctionName
{
    [PropertyOrder(0)] VList<IPersistentComment> Comments { get; }
    [PropertyOrder(1)] IType? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] VList<IParameter> Params { get; }
}

[VmfModel]
public partial interface IFunctionDecl : ICodeElement, IWithFunctionName, IControlFlowScope
{
    [PropertyOrder(0)] VList<IPersistentComment> Comments { get; }
    [PropertyOrder(1)] IType? ReturnType { get; set; }
    [PropertyOrder(2)] new string? FunctionName { get; set; }
    [PropertyOrder(3)] new VList<IStatement> Statements { get; }
    [PropertyOrder(4)] VList<IParameter> Params { get; }
}

// --- statements ---------------------------------------------------------------

[VmfModel][InterfaceOnly]
public partial interface IStatement : ICodeElement, IWithId, IControlFlowChildNode { }

[VmfModel]
public partial interface IBlockStatement : IStatement, IControlFlowScope
{
    [PropertyOrder(0)] new VList<IStatement> Statements { get; }
}

[VmfModel]
public partial interface IIfElseStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Condition { get; set; }
    [PropertyOrder(1)] IStatement? IfBlock { get; set; }
    [PropertyOrder(2)] IStatement? ElseBlock { get; set; }
}

[VmfModel]
public partial interface IWhileStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Check { get; set; }
    [PropertyOrder(1)] IStatement? Block { get; set; }
}

[VmfModel]
public partial interface IForStatement : IStatement, IControlFlowContainer
{
    [PropertyOrder(0)] IExpression? Init { get; set; }
    [PropertyOrder(1)] IExpression? Check { get; set; }
    [PropertyOrder(2)] IExpression? Inc { get; set; }
    [PropertyOrder(3)] IStatement? Block { get; set; }
}

[VmfModel]
public partial interface IPrintStatement : IStatement
{
    [PropertyOrder(0)] IExpression? PrintExpression { get; set; }
    [PropertyOrder(1)] VList<IExpression> ValueExpressions { get; }
}

[VmfModel]
public partial interface IArrayDeclStatement : IStatement, IWithVarName, IDeclStatement, IWithArraySizes
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] new VList<string> ArraySizes { get; }
}

[VmfModel]
public partial interface IVariableAssignmentStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] IExpression? AssignmentExpression { get; set; }
}

[VmfModel]
public partial interface IVarDeclStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IArrayAssignmentStatement : IStatement, IWithVarName, IDeclStatement
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? AssignmentExpression { get; set; }
    [PropertyOrder(2)] VList<IExpression> ArrayIndices { get; }
}

[VmfModel]
public partial interface IReturnStatement : IStatement
{
    [PropertyOrder(0)] IExpression? ReturnValue { get; set; }
}

[VmfModel]
public partial interface IFunctionCallStatement : IStatement, IWithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] VList<IExpression> Args { get; }
}

[VmfModel]
public partial interface ICommentStatement : IStatement
{
    [PropertyOrder(0)] IPersistentComment? Comment { get; set; }
}

// --- expressions --------------------------------------------------------------

[VmfModel][InterfaceOnly]
public partial interface IExpression : ICodeElement, IWithId, IControlFlowChildNode { }

[VmfModel]
public partial interface IArrayAccessExpression : IExpression
{
    [PropertyOrder(0)] IExpression? ArrayVariableExpression { get; set; }
    [PropertyOrder(1)] VList<IExpression> ArrayIndices { get; }
}

[VmfModel]
public partial interface IFunctionCallExpression : IExpression, IWithFunctionName
{
    [PropertyOrder(0)] new string? FunctionName { get; set; }
    [PropertyOrder(1)] VList<IExpression> Args { get; }
}

[VmfModel]
public partial interface INotExpression : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

[VmfModel]
public partial interface IAddressOperator : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

[VmfModel]
public partial interface IDereferenceOperator : IExpression
{
    [PropertyOrder(0)] IExpression? OperatorExpression { get; set; }
}

[VmfModel]
public partial interface ICastOperatorExpression : IExpression
{
    [PropertyOrder(0)] IType? CastType { get; set; }
    [PropertyOrder(1)] IExpression? OperatorExpression { get; set; }
}

[VmfModel]
public partial interface IMultExpression : IExpression
{
    [PropertyOrder(0)] IExpression? Left { get; set; }
    [PropertyOrder(1)] IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IDivExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IAddExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface ISubExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface ILtExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IAndExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface INonEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface ILtEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IGtEqualExpression : IExpression, IBinaryOperator
{
    [PropertyOrder(0)] new IExpression? Left { get; set; }
    [PropertyOrder(1)] new IExpression? Right { get; set; }
}

[VmfModel]
public partial interface IAssignmentExpression : IExpression, IDeclStatement, IWithVarName
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] new string? VarName { get; set; }
    [PropertyOrder(2)] IExpression? Assignment { get; set; }
}

[VmfModel]
public partial interface IAssignmentPlusExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? Assignment { get; set; }
}

[VmfModel]
public partial interface IAssignmentMinusExpression : IExpression
{
    [PropertyOrder(0)] string? VarName { get; set; }
    [PropertyOrder(1)] IExpression? Assignment { get; set; }
}

[VmfModel]
public partial interface IIncPostExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IDecPostExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IIncPreExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IDecPreExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IIdentifierExpression : IExpression, IWithVarName
{
    [PropertyOrder(0)] new string? VarName { get; set; }
}

[VmfModel]
public partial interface IIntExpression : IExpression, IConstExpression { }

[VmfModel]
public partial interface IDoubleExpression : IExpression, IConstExpression { }

[VmfModel]
public partial interface IBooleanExpression : IExpression, IConstExpression { }

[VmfModel]
public partial interface IStringExpression : IExpression, IConstExpression { }

[VmfModel]
public partial interface IParenExpression : IExpression
{
    [PropertyOrder(0)] IExpression? ParanExpr { get; set; }
}

// --- leaves -------------------------------------------------------------------

[VmfModel]
public partial interface IParameter : ICodeElement, IWithVarName, IWithArraySizes, IWithId, IDeclStatement
{
    [PropertyOrder(0)] new IType? DeclType { get; set; }
    [PropertyOrder(1)] string? Pointer { get; set; }
    [PropertyOrder(2)] new string? VarName { get; set; }
    [PropertyOrder(3)] new VList<string> ArraySizes { get; }
}

[VmfModel]
public partial interface IType : ICodeElement
{
    [PropertyOrder(0)] string? TypeName { get; set; }
}

[VmfModel]
public partial interface IPersistentComment : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] string? Text { get; set; }
}

[VmfModel]
public partial interface IIntLiteral : ICodeElement
{
    [VmfDefaultValue("null")]
    [PropertyOrder(0)] int? Value { get; set; }
}

/// <summary>
/// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.miniclang
/// .ControlFlowChildNodeDelegate. Declared at IVObject, as Java declares it at VObject, so the
/// one delegate serves every type that inherits ParentScopes.
/// </summary>
public sealed class ControlFlowChildNodeDelegate : IDelegatedBehavior<IVObject>
{
    private IVObject? _obj;

    public void SetCaller(IVObject caller) => _obj = caller;

    public VList<IControlFlowScope>? ParentScopes() => null;
}
