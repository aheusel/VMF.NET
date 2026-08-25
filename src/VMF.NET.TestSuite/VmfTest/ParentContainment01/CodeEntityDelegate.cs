// Ported from eu.mihosoft.vmftest.parentcontainment01.CodeEntityDelegate.
//
// The instantiation hook registers the change listener that populates Parent -- the model
// itself declares no containment at all.

using System.Linq;
using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.ParentContainment01;

public sealed class CodeEntityDelegate : IDelegatedBehavior<ICodeEntity>
{
    private ICodeEntity? _codeEntity;

    public void SetCaller(ICodeEntity caller) => _codeEntity = caller;

    public void OnCodeEntityInstantiated()
    {
        _codeEntity!.VMF.Changes.AddListener(l =>
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
