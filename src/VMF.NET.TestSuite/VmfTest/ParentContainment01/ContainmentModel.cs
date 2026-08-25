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
interface CodeEntity
{
    CodeEntity? Parent { get; set; }

    [DelegateTo(typeof(CodeEntityDelegate))]
    CodeEntity? Root();
}

[InterfaceOnly]
interface Expression : CodeEntity
{
}

interface OperatorExpression : Expression
{
    Expression? Left { get; set; }
    Expression? Right { get; set; }
}

interface NumberExpression : Expression
{
    double? Value { get; set; }
}
