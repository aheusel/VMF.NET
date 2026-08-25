// Ported from eu.mihosoft.vmftest.equals.vmfmodel.EqualsTestModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Equals.VmfModel;

[VmfModel(Equality = EqualsType.ContainmentAndExternal)]
interface IWithName
{
    string? Name { get; set; }
}

interface IEqualsTestModel : IWithName
{
    IAReference? Reference { get; set; }

    [Contains("IChild.Parent")]
    IChild? Child { get; set; }
}

interface IAReference : IWithName
{
}

interface IChild : IWithName
{
    [Container("IEqualsTestModel.Child")]
    IEqualsTestModel? Parent { get; }
}

interface IEqualsTestModel2 : IWithName
{
    int Value { get; set; }
}

[VmfEquals(EqualsType.All)]
interface IEqualsTestModelAllEq : IWithName
{
    int Value { get; set; }
    IAReference? Reference { get; set; }
}

[VmfEquals(EqualsType.Instance)]
interface IEqualsTestModelInstanceEq : IWithName
{
    int Value { get; set; }
    IAReference? Reference { get; set; }
}

[VmfEquals(EqualsType.ContainmentAndExternal)]
interface IEqualsTestContainmentEqListChild : IWithName
{
    [Container("IEqualsTestContainmentEqList.Children")]
    IEqualsTestContainmentEqList? Parent { get; }
}

[VmfEquals(EqualsType.ContainmentAndExternal)]
interface IEqualsTestContainmentEqList : IWithName
{
    [Contains("IEqualsTestContainmentEqListChild.Parent")]
    IEqualsTestContainmentEqListChild[] Children { get; }
}

[VmfEquals(EqualsType.Instance)]
interface IEqualsTestInstanceEqListChild : IWithName
{
    [Container("IEqualsTestInstanceEqList.Children")]
    IEqualsTestInstanceEqList? Parent { get; }
}

[VmfEquals(EqualsType.Instance)]
interface IEqualsTestInstanceEqList : IWithName
{
    [Contains("IEqualsTestInstanceEqListChild.Parent")]
    IEqualsTestInstanceEqListChild[] Children { get; }
}
