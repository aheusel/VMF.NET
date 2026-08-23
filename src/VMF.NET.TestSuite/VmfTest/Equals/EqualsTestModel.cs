// Ported from eu.mihosoft.vmftest.equals.vmfmodel.EqualsTestModel

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Equals;

[VmfModel(Equality = EqualsType.ContainmentAndExternal)]
public partial interface IWithName
{
    string? Name { get; set; }
}

[VmfModel]
public partial interface IEqualsTestModel : IWithName
{
    IAReference? Reference { get; set; }

    [Contains("IChild.Parent")]
    IChild? Child { get; set; }
}

[VmfModel]
public partial interface IAReference : IWithName
{
}

[VmfModel]
public partial interface IChild : IWithName
{
    [Container("IEqualsTestModel.Child")]
    IEqualsTestModel? Parent { get; }
}

[VmfModel]
public partial interface IEqualsTestModel2 : IWithName
{
    int Value { get; set; }
}

[VmfModel]
[VmfEquals(EqualsType.All)]
public partial interface IEqualsTestModelAllEq : IWithName
{
    int Value { get; set; }
    IAReference? Reference { get; set; }
}

[VmfModel]
[VmfEquals(EqualsType.Instance)]
public partial interface IEqualsTestModelInstanceEq : IWithName
{
    int Value { get; set; }
    IAReference? Reference { get; set; }
}

[VmfModel]
[VmfEquals(EqualsType.ContainmentAndExternal)]
public partial interface IEqualsTestContainmentEqListChild : IWithName
{
    [Container("IEqualsTestContainmentEqList.Children")]
    IEqualsTestContainmentEqList? Parent { get; }
}

[VmfModel]
[VmfEquals(EqualsType.ContainmentAndExternal)]
public partial interface IEqualsTestContainmentEqList : IWithName
{
    [Contains("IEqualsTestContainmentEqListChild.Parent")]
    VList<IEqualsTestContainmentEqListChild> Children { get; }
}

[VmfModel]
[VmfEquals(EqualsType.Instance)]
public partial interface IEqualsTestInstanceEqListChild : IWithName
{
    [Container("IEqualsTestInstanceEqList.Children")]
    IEqualsTestInstanceEqList? Parent { get; }
}

[VmfModel]
[VmfEquals(EqualsType.Instance)]
public partial interface IEqualsTestInstanceEqList : IWithName
{
    [Contains("IEqualsTestInstanceEqListChild.Parent")]
    VList<IEqualsTestInstanceEqListChild> Children { get; }
}
