// Ported from eu.mihosoft.vmf.VMFGenerateRuns -- the facts that set up Named/Child/Parent:
// testContainerContainmentAddChild, testToStringFeatureSimple, testDeepClone1.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Test2;

public class ParentChildTest
{
    [Fact]
    public void TestContainerContainmentAddChild()
    {
        var aParent = IParent.NewInstance();
        var aChild = IChild.NewInstance();

        aParent.Name = "Father";
        aParent.Children.Add(aChild);
        Assert.Equal("Father", aChild.Parent!.Name);

        aChild.Name = "Luke";
        Assert.Equal("Luke", aParent.Children[0].Name);

        aParent.Children.Clear();
        Assert.Null(aChild.Parent);
    }

    [Fact]
    public void TestToStringFeatureSimple()
    {
        var aParent = IParent.NewInstance();
        var aChild = IChild.NewInstance();

        aParent.Name = "Father";
        aParent.Children.Add(aChild);
        aChild.Name = "Luke";

        Assert.Equal(
            "{\"@type\":\"Parent\", \"Children\": [{\"@type\":\"Child\", \"Name\": \"Luke\"}], \"Elements\": [], \"Name\": \"Father\"}",
            aParent.ToString());
    }

    [Fact]
    public void TestDeepClone1()
    {
        var aParent = IParent.NewInstance();
        var aChild = IChild.NewInstance();

        aParent.Name = "Father";
        aParent.Children.Add(aChild);
        aChild.Name = "Luke";

        var aCloneParent = aParent.Vmf().Content().DeepCopy<IParent>();

        Assert.Equal(aParent, aCloneParent);
        Assert.Equal(aParent.ToString(), aCloneParent.ToString());
    }
}
