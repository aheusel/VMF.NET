// Ported from eu.mihosoft.vmftest.equals.EqualsTest
//
// Covers the three equality strategies:
//   ContainmentAndExternal (the model default here) -- own + contained state, cross-refs and
//                                                      containment parents ignored
//   All                    -- everything, including cross-references
//   Instance               -- reference identity; content equality still available through
//                             VMF.Content.ContentEquals(...)

using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Equals;

public class EqualsTest
{
    [Fact]
    public void TestEquals1()
    {
        // ContainmentAndExternal: identical properties => equal, even for distinct objects
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name1";

        Assert.Equal(model1, model2);
    }

    [Fact]
    public void TestEquals2()
    {
        // differing properties => not equal
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name2";

        Assert.NotEqual(model1, model2);
    }

    [Fact]
    public void TestEquals3()
    {
        // ContainmentAndExternal only considers containment and external types,
        // so a differing cross-reference does not break equality
        {
            var model1 = EqualsTestModel.NewInstance();
            model1.Name = "name1";
            model1.Reference = AReference.NewBuilder().WithName("ref name").Build();
            var model2 = EqualsTestModel.NewInstance();
            model2.Name = "name1";

            Assert.Equal(model1, model2);
        }
        {
            var model1 = EqualsTestModel.NewInstance();
            model1.Name = "name1";
            model1.Reference = AReference.NewBuilder().WithName("ref name 1").Build();
            var model2 = EqualsTestModel.NewInstance();
            model2.Name = "name1";
            model2.Reference = AReference.NewBuilder().WithName("ref name 2").Build();

            Assert.Equal(model1, model2);
        }
    }

    [Fact]
    public void TestEquals4()
    {
        // a differing CONTAINED child does break equality
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        model1.Child = Child.NewBuilder().WithName("child name").Build();
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name1";

        Assert.NotEqual(model1, model2);
    }

    [Fact]
    public void TestEquals5()
    {
        // equal children => equal parents
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        model1.Child = Child.NewBuilder().WithName("child name").Build();
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name1";
        model2.Child = Child.NewBuilder().WithName("child name").Build();

        Assert.Equal(model1, model2);
    }

    [Fact]
    public void TestEquals6()
    {
        // children stay equal even when their parents are not: the child's reference to the
        // parent is the containment parent side, which equality ignores
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        model1.Child = Child.NewBuilder().WithName("child name").Build();
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name2";
        model2.Child = Child.NewBuilder().WithName("child name").Build();

        Assert.NotEqual(model1, model2);
        Assert.Equal(model1.Child, model2.Child);
    }

    [Fact]
    public void TestEquals7()
    {
        // ...but children with differing own properties are not equal
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        model1.Child = Child.NewBuilder().WithName("child name 1").Build();
        var model2 = EqualsTestModel.NewInstance();
        model2.Name = "name2";
        model2.Child = Child.NewBuilder().WithName("child name 2").Build();

        Assert.NotEqual(model1.Child, model2.Child);
    }

    [Fact]
    public void TestEqualsContract1_Reflexive()
    {
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        model1.Child = Child.NewBuilder().WithName("child name 1").Build();

        Assert.Equal(model1, model1);
    }

    [Fact]
    public void TestEqualsContract2_Symmetric()
    {
        // x eq y  <=>  y eq x, including when compared through a shared supertype
        var model1 = EqualsTestModel.NewInstance();
        model1.Name = "name1";
        var model2 = EqualsTestModel2.NewInstance();
        model2.Name = "name1";

        Assert.NotEqual<object>(model1, model2);
        Assert.NotEqual<object>(model2, model1);

        WithName withName1 = model1;
        WithName withName2 = model2;
        Assert.NotEqual(withName1, withName2);

        var withName3 = WithName.NewBuilder().WithName("name1").Build();
        Assert.NotEqual(withName3, withName1);
        Assert.NotEqual(withName3, withName2);
        Assert.NotEqual(withName1, withName3);
        Assert.NotEqual(withName2, withName3);
    }

    [Fact]
    public void TestEqualsContract3_Transitive()
    {
        // x eq y && y eq z  =>  x eq z
        var x = EqualsTestModel.NewInstance();
        x.Name = "name1";
        x.Child = Child.NewBuilder().WithName("child name 1").Build();
        var y = EqualsTestModel.NewInstance();
        y.Name = "name1";
        y.Child = Child.NewBuilder().WithName("child name 1").Build();
        var z = EqualsTestModel.NewInstance();
        z.Name = "name1";
        z.Child = Child.NewBuilder().WithName("child name 1").Build();

        Assert.Equal(x, y);
        Assert.Equal(y, z);
        Assert.Equal(x, z);
    }

    [Fact]
    public void TestEqualsAll()
    {
        // [VmfEquals(All)]: identical state => equal
        {
            var model1 = EqualsTestModelAllEq.NewBuilder().WithName("my name1").WithValue(3).Build();
            var model2 = EqualsTestModelAllEq.NewInstance();
            EqualsTestModelAllEq.NewBuilder().ApplyFrom(model1).ApplyTo(model2);

            Assert.Equal(model1, model2);
        }
        // ...and All DOES consider cross-references, so differing references break equality.
        // (The Java original sets model1's reference twice -- clearly meant for model2 --
        //  which makes the fact pass for the wrong reason. Ported as intended.)
        {
            var model1 = EqualsTestModelAllEq.NewInstance();
            model1.Name = "name1";
            model1.Reference = AReference.NewBuilder().WithName("ref name 1").Build();
            var model2 = EqualsTestModelAllEq.NewInstance();
            model2.Name = "name1";
            model2.Reference = AReference.NewBuilder().WithName("ref name 2").Build();

            Assert.NotEqual(model1, model2);
        }
    }

    [Fact]
    public void TestEqualsInstance()
    {
        // [VmfEquals(Instance)]: identical state, still not equal -- identity only
        {
            var model1 = EqualsTestModelInstanceEq.NewBuilder().WithName("my name1").WithValue(3).Build();
            var model2 = EqualsTestModelInstanceEq.NewInstance();
            EqualsTestModelInstanceEq.NewBuilder().ApplyFrom(model1).ApplyTo(model2);

            Assert.NotEqual(model1, model2);
        }
        // with Instance equality, content comparison is still available via Content(),
        // which uses the ContainmentAndExternal semantics (cross-refs ignored)
        {
            var model1 = EqualsTestModelInstanceEq.NewBuilder().WithName("my name1").WithValue(3).Build();
            model1.Reference = AReference.NewBuilder().WithName("ref name 1").Build();
            var model2 = EqualsTestModelInstanceEq.NewBuilder().WithName("my name1").WithValue(3).Build();
            model2.Reference = AReference.NewBuilder().WithName("ref name 2").Build();

            Assert.True(model1.VMF.Content.ContentEquals(model2));
        }
    }

    [Fact]
    public void TestEqualContainmentEq()
    {
        // ContainmentAndExternal over a contained LIST: a deep copy is equal to its original
        var model1 = EqualsTestContainmentEqList.NewBuilder()
            .WithName("my name1")
            .WithChildren(
                EqualsTestContainmentEqListChild.NewBuilder().WithName("Child 1").Build(),
                EqualsTestContainmentEqListChild.NewBuilder().WithName("Child 2").Build())
            .Build();

        var model2 = model1.VMF.Content.DeepCopy<EqualsTestContainmentEqList>();

        Assert.Equal(model1, model2);
    }

    [Fact]
    public void TestEqualInstanceEq()
    {
        // Instance equality over a contained list: a deep copy is a DIFFERENT instance...
        var model1 = EqualsTestInstanceEqList.NewBuilder()
            .WithName("my name1")
            .WithChildren(
                EqualsTestInstanceEqListChild.NewBuilder().WithName("Child 1").Build(),
                EqualsTestInstanceEqListChild.NewBuilder().WithName("Child 2").Build())
            .Build();

        var model2 = model1.VMF.Content.DeepCopy<EqualsTestInstanceEqList>();

        Assert.NotEqual(model1, model2);

        // ...but its content is identical
        Assert.True(model1.VMF.Content.ContentEquals(model2));
    }
}
