// Ported from eu.mihosoft.vmftest.builders.BuilderTest

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Builders;

public class BuilderTest
{
    [Fact(Skip = "Needs builder-accepting With* overloads. The Java fact passes UNBUILT nested " +
                 "builders (Child.newBuilder().withValue(1)) straight to withChildren/withChild " +
                 "and VMF builds them lazily on build(). VMF.NET generates With* overloads that " +
                 "take built instances only.")]
    public void TestWithNestedBuilders()
    {
    }

    [Fact]
    public void TestWithProperties()
    {
        var b = IAClass.NewBuilder()
            .WithName("my name")
            .WithIds("id1", "id2", "id3")
            .WithChildren(
                IChild.NewBuilder().WithValue(1).Build(),
                IChild.NewBuilder().WithValue(2).Build(),
                IChild.NewBuilder().WithValue(3).Build())
            .WithChild(
                IChild2.NewBuilder().WithValue(4).Build());

        var anInstance = b.Build();

        Assert.Equal("my name", anInstance.Name);
        Assert.Equal(3, anInstance.Ids.Count);
        Assert.Equal("id1", anInstance.Ids[0]);
        Assert.Equal("id2", anInstance.Ids[1]);
        Assert.Equal("id3", anInstance.Ids[2]);
        Assert.Equal(3, anInstance.Children.Count);
        Assert.Equal(1, anInstance.Children[0].Value);
        Assert.Equal(2, anInstance.Children[1].Value);
        Assert.Equal(3, anInstance.Children[2].Value);
        Assert.Equal(4, anInstance.Child!.Value);
    }
}
