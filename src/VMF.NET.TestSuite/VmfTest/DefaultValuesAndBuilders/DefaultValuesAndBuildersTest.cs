// Ported from eu.mihosoft.vmftest.defaultvaluesandbuilders.DefaultValuesAndBuildersTest
//
// Defaults must apply whether the object comes from NewInstance() or an untouched builder.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.DefaultValuesAndBuilders;

public class DefaultValuesAndBuildersTest
{
    [Fact]
    public void DefaultValues_ApplyToBothConstructionPaths()
    {
        var withValuesInstance = IWithDefaultValues.NewInstance();
        var withValuesBuilder = IWithDefaultValues.NewBuilder().Build();

        Assert.Equal("my name", withValuesInstance.Name);
        Assert.True(withValuesInstance.Visible);

        Assert.Equal("my name", withValuesBuilder.Name);
        Assert.True(withValuesBuilder.Visible);
    }
}
