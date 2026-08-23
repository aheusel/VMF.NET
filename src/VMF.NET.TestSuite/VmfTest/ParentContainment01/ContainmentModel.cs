// Ported from eu.mihosoft.vmftest.parentcontainment01.vmfmodel.ContainmentTest
//
// DEVIATION: Java declares root() on the [InterfaceOnly] CodeEntity and relies on it being
// inherited by every subtype. VMF.NET does not generate INHERITED [DelegateTo] methods, so
// the concrete types re-declare it. CodeEntityDelegate therefore implements
// IDelegatedBehavior<T> once per model type that delegates to it.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ParentContainment01;

[VmfModel]
[InterfaceOnly]
[DelegateTo(typeof(CodeEntityDelegate))]
public partial interface ICodeEntity
{
    ICodeEntity? Parent { get; set; }

    [DelegateTo(typeof(CodeEntityDelegate))]
    ICodeEntity? Root();
}

[VmfModel]
[InterfaceOnly]
public partial interface IExpression : ICodeEntity
{
}

[VmfModel]
public partial interface IOperatorExpression : IExpression
{
    IExpression? Left { get; set; }
    IExpression? Right { get; set; }

    [DelegateTo(typeof(CodeEntityDelegate))]
    new ICodeEntity? Root();
}

[VmfModel]
public partial interface INumberExpression : IExpression
{
    double? Value { get; set; }

    [DelegateTo(typeof(CodeEntityDelegate))]
    new ICodeEntity? Root();
}

/// <summary>
/// Walks up the <see cref="ICodeEntity.Parent"/> chain to the root entity.
/// </summary>
public sealed class CodeEntityDelegate
    : IDelegatedBehavior<ICodeEntity>,
      IDelegatedBehavior<IOperatorExpression>,
      IDelegatedBehavior<INumberExpression>
{
    private ICodeEntity? _caller;

    void IDelegatedBehavior<ICodeEntity>.SetCaller(ICodeEntity caller) => _caller = caller;
    void IDelegatedBehavior<IOperatorExpression>.SetCaller(IOperatorExpression caller) => _caller = caller;
    void IDelegatedBehavior<INumberExpression>.SetCaller(INumberExpression caller) => _caller = caller;

    public ICodeEntity? Root()
    {
        var current = _caller;
        while (current?.Parent is not null)
        {
            current = current.Parent;
        }
        return current;
    }
}
