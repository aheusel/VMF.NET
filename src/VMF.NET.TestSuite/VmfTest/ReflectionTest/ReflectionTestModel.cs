// Ported from eu.mihosoft.vmftests.reflectiontest.vmfmodel
//
// Java expresses ReflectionTest.getValues()'s default as a Java expression
// (VList.newInstance(Arrays.asList("a","b","c"))); the C# equivalent is a collection
// initialiser expression, evaluated on first access in the same way.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ReflectionTest.VmfModel;

interface IInheritedDefaultValueParent
{
    [VmfDefaultValue("123")]
    int MyValue { get; set; }
}

interface IInheritedDefaultValueParent2
{
    [VmfDefaultValue("456")]
    int MyValue { get; set; }
}

interface IInheritedDefaultValue : IInheritedDefaultValueParent
{
}

// DEVIATION: Java lets a type inherit the same member from two unrelated interfaces; C#
// reports CS0229 (ambiguous). The member is therefore re-declared, carrying the default of
// the parent listed first -- which is the one VMF's property collection would pick anyway.
interface IInheritedDefaultValueFromTwoParents
    : IInheritedDefaultValueParent, IInheritedDefaultValueParent2
{
    [VmfDefaultValue("123")]
    new int MyValue { get; set; }
}

interface IInheritedDefaultValueFromTwoParents2
    : IInheritedDefaultValueParent2, IInheritedDefaultValueParent
{
    [VmfDefaultValue("456")]
    new int MyValue { get; set; }
}

interface IInheritedDefaultValueOverride : IInheritedDefaultValueParent
{
    [VmfDefaultValue("-123")]
    new int MyValue { get; set; }
}

interface IInheritedDefaultValueOverride2 : IInheritedDefaultValueParent
{
    // should default to 0 (the default for int)
    new int MyValue { get; set; }
}

interface INode
{
    [Contains("INode.Parent")]
    INode[] Children { get; }

    [Container("INode.Children")]
    INode? Parent { get; }
}

interface IReflectionTest
{
    [VmfDefaultValue("23")]
    int Id { get; set; }

    [VmfDefaultValue("new[] { \"a\", \"b\", \"c\" }")]
    string[] Values { get; }

    string? Id2 { get; set; }
}
