// Ported from eu.mihosoft.vmftest.propertytype.PropertyTypeTest
//
// Reflected property types must report list-ness, model-ness, the full type name, and (for
// lists only) the element type name.

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.PropertyType;

public class PropertyTypeTest
{
    private const string Ns = "VMF.NET.TestSuite.VmfTest.PropertyType.";

    [Fact]
    public void TestPropertyTypes()
    {
        var e = IEntityWithProperties.NewInstance();
        var cE = IChildEntity.NewInstance();

        var ids = e.Vmf().Reflect().PropertyByName("Ids");
        var children = e.Vmf().Reflect().PropertyByName("Children");
        var entity = e.Vmf().Reflect().PropertyByName("Entity");
        var name = cE.Vmf().Reflect().PropertyByName("Name");

        Assert.NotNull(ids);
        Assert.NotNull(children);
        Assert.NotNull(entity);
        Assert.NotNull(name);

        // list of a non-model type
        Assert.True(ids!.Type.IsListType, "Ids is a list type but is not flagged as such");
        Assert.False(ids.Type.IsModelType, "Ids is no model type but is flagged as such");

        // list of a model type
        Assert.True(children!.Type.IsListType, "Children is a list type but is not flagged as such");
        Assert.True(children.Type.IsModelType, "Children is a model type but is not flagged as such");

        // single model type
        Assert.False(entity!.Type.IsListType, "Entity is no list type but is flagged as such");
        Assert.True(entity.Type.IsModelType, "Entity is a model type but is not flagged as such");
        Assert.Equal(Ns + "IChildEntity", entity.Type.Name);

        // plain scalar
        Assert.False(name!.Type.IsListType, "Name is no list type but is flagged as such");
        Assert.False(name.Type.IsModelType, "Name is no model type but is flagged as such");

        // an element type name is reported for lists only
        Assert.Null(name.Type.GetElementTypeName());
        Assert.Equal(Ns + "IChildEntity", children.Type.GetElementTypeName());
    }
}
