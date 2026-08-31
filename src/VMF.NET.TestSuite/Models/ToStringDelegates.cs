// Behaviour classes for ToStringDelegationModel. Ported from the Java tutorial's
// StoreDelegate / ItemDelegate, which live in the generated-into package for the same reason
// these live beside the generated API: they refer to the generated types.

using System.Text;
using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.Models;

public class ItemDelegate : IDelegatedBehavior<Item>
{
    private Item _caller = null!;

    public void SetCaller(Item caller) => _caller = caller;

    public override string ToString() => "item: " + _caller.Id;
}

public class StoreDelegate : IDelegatedBehavior<Store>
{
    private Store _caller = null!;

    public void SetCaller(Store caller) => _caller = caller;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("> store: " + _caller.Id + " \n");
        foreach (var i in _caller.Items)
        {
            sb.Append(" -> " + i).Append('\n');
        }
        return sb.ToString();
    }
}
