// Ported from eu.mihosoft.vmftest.externaltypes.ExternalTypesTest
//
// DEVIATION: Java declares stand-in interfaces (@ExternalType) for java.util.List and for the
// action type. The C# model references real .NET types directly, so the facts exercise the
// same thing -- an external type as a scalar property, as a list element, and as a delegated
// method parameter -- against MyType/MyAction rather than List.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.ExternalTypes;

public class ExternalTypesTest
{
    [Fact]
    public void BasicListTypeTest()
    {
        var model = Model.NewInstance();

        // an external type is usable as a scalar property and as a list element
        model.Entry = new MyType { Name = "single" };
        model.Entries.Add(new MyType { Name = "in list" });

        Assert.Equal("single", model.Entry!.Name);
        Assert.Single(model.Entries);
        Assert.Equal("in list", model.Entries[0].Name);
    }

    [Fact]
    public void CustomActionTypeTest()
    {
        var model = Model.NewInstance();

        // the delegated method takes an external functional type and applies it to the caller
        model.RunAction(m => m.Entries.Add(new MyType()));

        // Expected exactly one list entry
        Assert.Single(model.Entries);
    }
}
