// Ported from eu.mihosoft.vmftest.propertytype.vmfmodel.EntityWithProperties

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyType;

[VmfModel]
public partial interface IEntityWithProperties
{
    VList<int> Ids { get; }
    VList<IChildEntity> Children { get; }
    IChildEntity? Entity { get; set; }
}

[VmfModel]
public partial interface IChildEntity
{
    string? Name { get; set; }
}
