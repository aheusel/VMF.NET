// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.miniclang
// .ControlFlowChildNodeDelegate.

using System.Linq;
using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.Generated.MiniCLang;

/// <summary>
/// Ported from eu.mihosoft.vmftest.complex.vmf_text.generated.miniclang
/// .ControlFlowChildNodeDelegate. Declared at IVObject, as Java declares it at VObject, so the
/// one delegate serves every type that inherits ParentScopes.
/// </summary>
public sealed class ControlFlowChildNodeDelegate : IDelegatedBehavior<IVObject>
{
    private IVObject? _obj;

    public void SetCaller(IVObject caller) => _obj = caller;

    public VList<ControlFlowScope>? ParentScopes() => null;
}
