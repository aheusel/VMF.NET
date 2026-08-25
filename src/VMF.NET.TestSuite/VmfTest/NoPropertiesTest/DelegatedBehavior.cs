// Behaviour delegate for the NoProperties model. Lives beside the generated API rather
// than in the model namespace, as Java's delegates live in the package VMF generates into.

using VMF.NET.Runtime;

namespace VMF.NET.TestSuite.VmfTest.NoPropertiesTest;

public sealed class DelegatedBehavior : IDelegatedBehavior<NoProperties>
{
    private NoProperties? _caller;
    public void SetCaller(NoProperties caller) => _caller = caller;

    public int CallCount { get; private set; }
    public void TestDelegation() => CallCount++;
}
