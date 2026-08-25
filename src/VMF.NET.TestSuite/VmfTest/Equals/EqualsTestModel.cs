// Ported from eu.mihosoft.vmftest.equals.vmfmodel.EqualsTestModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Equals.VmfModel;

[VmfModel(Equality = EqualsType.ContainmentAndExternal)]
interface WithName
{
    string? Name { get; set; }
}

interface EqualsTestModel : WithName
{
    AReference? Reference { get; set; }

    [Contains("Child.Parent")]
    Child? Child { get; set; }
}

interface AReference : WithName
{
}

interface Child : WithName
{
    [Container("EqualsTestModel.Child")]
    EqualsTestModel? Parent { get; }
}

interface EqualsTestModel2 : WithName
{
    int Value { get; set; }
}

[VmfEquals(EqualsType.All)]
interface EqualsTestModelAllEq : WithName
{
    int Value { get; set; }
    AReference? Reference { get; set; }
}

[VmfEquals(EqualsType.Instance)]
interface EqualsTestModelInstanceEq : WithName
{
    int Value { get; set; }
    AReference? Reference { get; set; }
}

[VmfEquals(EqualsType.ContainmentAndExternal)]
interface EqualsTestContainmentEqListChild : WithName
{
    [Container("EqualsTestContainmentEqList.Children")]
    EqualsTestContainmentEqList? Parent { get; }
}

[VmfEquals(EqualsType.ContainmentAndExternal)]
interface EqualsTestContainmentEqList : WithName
{
    [Contains("EqualsTestContainmentEqListChild.Parent")]
    EqualsTestContainmentEqListChild[] Children { get; }
}

[VmfEquals(EqualsType.Instance)]
interface EqualsTestInstanceEqListChild : WithName
{
    [Container("EqualsTestInstanceEqList.Children")]
    EqualsTestInstanceEqList? Parent { get; }
}

[VmfEquals(EqualsType.Instance)]
interface EqualsTestInstanceEqList : WithName
{
    [Contains("EqualsTestInstanceEqListChild.Parent")]
    EqualsTestInstanceEqListChild[] Children { get; }
}
