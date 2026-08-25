// Ported from eu.mihosoft.vmftest.propertytype.vmfmodel.EntityWithProperties

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyType.VmfModel;

interface IEntityWithProperties
{
    int[] Ids { get; }
    IChildEntity[] Children { get; }
    IChildEntity? Entity { get; set; }
}

interface IChildEntity
{
    string? Name { get; set; }
}
