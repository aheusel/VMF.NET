// Ported from eu.mihosoft.vmftest.cross_ref.vmfmodel.CrossRef

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.CrossRef.VmfModel;

interface IEntityOneA
{
    [Refers("IEntityTwoA.Ref")]
    IEntityTwoA? Ref { get; set; }
}

interface IEntityTwoA
{
    [Refers("IEntityOneA.Ref")]
    IEntityOneA? Ref { get; set; }
}

interface IEntityOneB
{
    [Refers("IEntityTwoB.Refs")]
    IEntityTwoB? Ref { get; set; }
}

interface IEntityTwoB
{
    [Refers("IEntityOneB.Ref")]
    VList<IEntityOneB> Refs { get; }
}

interface IEntityOneC
{
    [Refers("IEntityTwoC.Refs")]
    VList<IEntityTwoC> Refs { get; }
}

interface IEntityTwoC
{
    [Refers("IEntityOneC.Refs")]
    VList<IEntityOneC> Refs { get; }
}
