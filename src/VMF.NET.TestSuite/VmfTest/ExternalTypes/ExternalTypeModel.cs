// Ported from eu.mihosoft.vmftest.externaltypes.vmfmodel.ExternalTypeModel
//
// Java declares stand-in interfaces (@ExternalType) for types outside the model, e.g.
// java.util.List. In C# an ordinary .NET type can be referenced directly, so the model names
// MyType and MyAction straight -- they live beside the generated API, in ExternalTypes.cs.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ExternalTypes.VmfModel;

interface IModel
{
    string? Name { get; set; }
    MyType? Entry { get; set; }
    MyType[] Entries { get; }

    [DelegateTo(typeof(ModelBehavior))]
    void RunAction(MyAction action);
}
