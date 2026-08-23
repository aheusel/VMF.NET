// Ported from eu.mihosoft.vmftest.defaultvaluesandbuilders.vmfmodel.DefaultValuesAndbuilders

using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.DefaultValuesAndBuilders;

[VmfModel]
public partial interface IWithDefaultValues
{
    [VmfDefaultValue("true")]
    bool Visible { get; set; }

    [VmfDefaultValue("\"my name\"")]
    string? Name { get; set; }
}
