// Ported from eu.mihosoft.vmftest.propertytype.vmfmodel.EntityWithProperties

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.PropertyType.VmfModel;

interface EntityWithProperties
{
    int[] Ids { get; }
    ChildEntity[] Children { get; }
    ChildEntity? Entity { get; set; }
}

interface ChildEntity
{
    string? Name { get; set; }
}
