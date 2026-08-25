// Ported from eu.mihosoft.vmftest.cross_ref.vmfmodel.CrossRef

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.CrossRef.VmfModel;

interface EntityOneA
{
    [Refers("EntityTwoA.Ref")]
    EntityTwoA? Ref { get; set; }
}

interface EntityTwoA
{
    [Refers("EntityOneA.Ref")]
    EntityOneA? Ref { get; set; }
}

interface EntityOneB
{
    [Refers("EntityTwoB.Refs")]
    EntityTwoB? Ref { get; set; }
}

interface EntityTwoB
{
    [Refers("EntityOneB.Ref")]
    EntityOneB[] Refs { get; }
}

interface EntityOneC
{
    [Refers("EntityTwoC.Refs")]
    EntityTwoC[] Refs { get; }
}

interface EntityTwoC
{
    [Refers("EntityOneC.Refs")]
    EntityOneC[] Refs { get; }
}
