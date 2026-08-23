// Ported from eu.mihosoft.vmftest.cross_ref.vmfmodel.CrossRef

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.CrossRef;

[VmfModel]
public partial interface IEntityOneA
{
    [Refers("IEntityTwoA.Ref")]
    IEntityTwoA? Ref { get; set; }
}

[VmfModel]
public partial interface IEntityTwoA
{
    [Refers("IEntityOneA.Ref")]
    IEntityOneA? Ref { get; set; }
}

[VmfModel]
public partial interface IEntityOneB
{
    [Refers("IEntityTwoB.Refs")]
    IEntityTwoB? Ref { get; set; }
}

[VmfModel]
public partial interface IEntityTwoB
{
    [Refers("IEntityOneB.Ref")]
    VList<IEntityOneB> Refs { get; }
}

[VmfModel]
public partial interface IEntityOneC
{
    [Refers("IEntityTwoC.Refs")]
    VList<IEntityTwoC> Refs { get; }
}

[VmfModel]
public partial interface IEntityTwoC
{
    [Refers("IEntityOneC.Refs")]
    VList<IEntityOneC> Refs { get; }
}
