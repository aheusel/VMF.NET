// Ported from eu.mihosoft.vmftest.parentcontainment01.vmfmodel.ContainmentTest
// and eu.mihosoft.vmftest.parentcontainment01.CodeEntityDelegate.
//
// The model declares no containment at all: CodeEntity carries a type-level [DelegateTo], and the
// delegate's instantiation hook registers the change listener that populates Parent.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ParentContainment01.VmfModel;

[InterfaceOnly]
[DelegateTo(typeof(CodeEntityDelegate))]
interface ICodeEntity
{
    ICodeEntity? Parent { get; set; }

    [DelegateTo(typeof(CodeEntityDelegate))]
    ICodeEntity? Root();
}

[InterfaceOnly]
interface IExpression : ICodeEntity
{
}

interface IOperatorExpression : IExpression
{
    IExpression? Left { get; set; }
    IExpression? Right { get; set; }
}

interface INumberExpression : IExpression
{
    double? Value { get; set; }
}
