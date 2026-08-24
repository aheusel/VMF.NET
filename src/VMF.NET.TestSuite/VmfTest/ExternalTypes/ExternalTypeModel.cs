// Ported from eu.mihosoft.vmftest.externaltypes.vmfmodel.ExternalTypeModel
//
// Java declares stand-in interfaces (@ExternalType) for types outside the model, e.g.
// java.util.List. In C# an ordinary .NET type can be referenced directly, so the
// stand-ins are plain classes here and [ExternalType] is kept only where Java had it.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ExternalTypes;

/// <summary>
/// Java declares <c>MyAction extends Consumer&lt;Model&gt;</c> -- a functional interface applied
/// to the caller. A delegate is the direct C# equivalent, so a lambda can be passed exactly as
/// the Java fact does.
/// </summary>
public delegate void MyAction(IModel model);

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

    public void RunAction(MyAction action) => action(_caller!);
}
