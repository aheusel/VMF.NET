// Ported from eu.mihosoft.vmftests.reflectiontest.vmfmodel
//
// Java expresses ReflectionTest.getValues()'s default as a Java expression
// (VList.newInstance(Arrays.asList("a","b","c"))); the C# equivalent is a collection
// initialiser expression, evaluated on first access in the same way.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.ReflectionTest.VmfModel;

interface InheritedDefaultValueParent
{
    [VmfDefaultValue("123")]
    int MyValue { get; set; }
}

interface InheritedDefaultValueParent2
{
    [VmfDefaultValue("456")]
    int MyValue { get; set; }
}

interface InheritedDefaultValue : InheritedDefaultValueParent
{
}

// DEVIATION: Java lets a type inherit the same member from two unrelated interfaces; C#
// reports CS0229 (ambiguous). The member is therefore re-declared, carrying the default of
// the parent listed first -- which is the one VMF's property collection would pick anyway.
interface InheritedDefaultValueFromTwoParents
    : InheritedDefaultValueParent, InheritedDefaultValueParent2
{
    [VmfDefaultValue("123")]
    new int MyValue { get; set; }
}

interface InheritedDefaultValueFromTwoParents2
    : InheritedDefaultValueParent2, InheritedDefaultValueParent
{
    [VmfDefaultValue("456")]
    new int MyValue { get; set; }
}

interface InheritedDefaultValueOverride : InheritedDefaultValueParent
{
    [VmfDefaultValue("-123")]
    new int MyValue { get; set; }
}

interface InheritedDefaultValueOverride2 : InheritedDefaultValueParent
{
    // should default to 0 (the default for int)
    new int MyValue { get; set; }
}

interface Node
{
    [Contains("Node.Parent")]
    Node[] Children { get; }

    [Container("Node.Children")]
    Node? Parent { get; }
}

interface ReflectionTest
{
    [VmfDefaultValue("23")]
    int Id { get; set; }

    [VmfDefaultValue("new[] { \"a\", \"b\", \"c\" }")]
    string[] Values { get; }

    string? Id2 { get; set; }
}
