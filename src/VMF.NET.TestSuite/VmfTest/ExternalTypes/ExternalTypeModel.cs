// Ported from eu.mihosoft.vmftest.externaltypes.vmfmodel.ExternalTypeModel
//
// Java declares stand-in interfaces (@ExternalType) for types outside the model, e.g.
// java.util.List. In C# an ordinary .NET type can be referenced directly, so the
// stand-ins are plain classes here and [ExternalType] is kept only where Java had it.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ExternalTypes;

public sealed class MyAction
{
    public string? Name { get; set; }
}

public sealed class MyType
{
    public string? Name { get; set; }
}

[VmfModel]
public partial interface IModel
{
    string? Name { get; set; }
    MyType? Entry { get; set; }
    VList<MyType> Entries { get; }

    [DelegateTo(typeof(ModelBehavior))]
    void RunAction(MyAction action);
}

/// <summary>Behavior delegate for <see cref="IModel.RunAction"/>.</summary>
public sealed class ModelBehavior : IDelegatedBehavior<IModel>
{
    private IModel? _caller;

    public void SetCaller(IModel caller) => _caller = caller;

    public void RunAction(MyAction action) => _caller!.Name = action.Name;
}
