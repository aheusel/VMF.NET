// Ported from eu.mihosoft.vmftest.parentcontainment01.vmfmodel.ContainmentTest
// and eu.mihosoft.vmftest.parentcontainment01.CodeEntityDelegate.
//
// The model declares no containment at all: CodeEntity carries a type-level [DelegateTo], and the
// delegate's instantiation hook registers the change listener that populates Parent.

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
}

[VmfModel]
public partial interface INumberExpression : IExpression
{
    double? Value { get; set; }
}

public sealed class CodeEntityDelegate : IDelegatedBehavior<ICodeEntity>
{
    private ICodeEntity? _codeEntity;

    public void SetCaller(ICodeEntity caller) => _codeEntity = caller;

    public void OnCodeEntityInstantiated()
    {
        _codeEntity!.Vmf().Changes().AddListener(l =>
        {
            if (l.Object != _codeEntity || "Parent" == l.PropertyName)
            {
                return;
            }

            object? o = l.PropertyChange!.NewValue;

            if (o is ICodeEntity cE)
            {
                cE.Parent = _codeEntity;
            }
        }, false);
    }

    public ICodeEntity? Root()
    {
        ICodeEntity? cE = _codeEntity;

        while (cE!.Parent != null)
        {
            cE = cE.Parent;
        }

        return cE;
    }
}
