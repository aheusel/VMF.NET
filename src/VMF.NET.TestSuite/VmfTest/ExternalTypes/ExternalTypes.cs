// The .NET types the model references, plus its behaviour delegate. Java declares stand-in
// interfaces (@ExternalType) for these; C# references the real types directly.

using VMF.NET.Runtime;

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

/// <summary>Behavior delegate for <see cref="IModel.RunAction"/>.</summary>
public sealed class ModelBehavior : IDelegatedBehavior<IModel>
{
    private IModel? _caller;

    public void SetCaller(IModel caller) => _caller = caller;

    public void RunAction(MyAction action) => action(_caller!);
}
