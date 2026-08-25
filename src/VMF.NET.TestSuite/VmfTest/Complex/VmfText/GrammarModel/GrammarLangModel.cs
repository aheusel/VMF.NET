// Ported from eu.mihosoft.vmftest.complex.vmf_text.grammarmodel.vmfmodel.GrammarLangModel
//
// Near-identical to the unparsermodel copy, but: no model-wide equality setting, and
// SuperClass/ChildClasses form a CONTAINMENT pair here (not cross-references).

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.GrammarModel.VmfModel;

interface GrammarModel
{
    [Contains("RuleClass.Model")]
    RuleClass[] RuleClasses { get; }

    string? GrammarName { get; set; }
    string? PackageName { get; set; }

    [Contains("TypeMappings.Model")]
    TypeMappings? TypeMappings { get; set; }

    [Contains("CustomRule.Model")]
    CustomRule[] CustomRules { get; }
}

interface CustomRule : WithText
{
    [Container("GrammarModel.CustomRules")]
    GrammarModel? Model { get; }
}

[Immutable]
interface CodeRange
{
    CodeLocation? Start { get; }
    CodeLocation? Stop { get; }
    int Length { get; }
}

[Immutable]
interface CodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[InterfaceOnly]
interface LangElement
{
}

[InterfaceOnly]
interface CodeElement
{
    CodeRange? CodeRange { get; set; }
}

[InterfaceOnly]
interface WithType : LangElement
{
    Type? Type { get; set; }
}

[InterfaceOnly]
interface WithName : LangElement
{
    [GetterOnly]
    string? Name { get; }
}

[Immutable]
interface Type : LangElement, WithName
{
    string? PackageName { get; }
    string? AntlrRuleName { get; }
    bool RuleType { get; }
    bool ArrayType { get; }
}

interface RuleClass : WithName, CodeElement
{
    [Container("GrammarModel.RuleClasses")]
    GrammarModel? Model { get; }

    [Contains("Property.Parent")]
    Property[] Properties { get; }

    [Contains("Property.Parent")]
    Property[] CustomProperties { get; }

    [Container("RuleClass.ChildClasses")]
    RuleClass? SuperClass { get; }

    string[] SuperInterfaces { get; }

    [Contains("RuleClass.SuperClass")]
    RuleClass[] ChildClasses { get; }

    bool Root { get; set; }

    [Contains("DelegationMethod.Parent")]
    DelegationMethod[] DelegationMethods { get; }

    [Contains("RuleAnnotation.Parent")]
    RuleAnnotation[] CustomRuleAnnotations { get; }
}

interface Property : WithName, WithType, CodeElement
{
    [Container("RuleClass.Properties")]
    RuleClass? Parent { get; }

    [Contains("PropertyAnnotation.Property")]
    PropertyAnnotation[] Annotations { get; }
}

interface DelegationMethod : WithText
{
    [Container("RuleClass.DelegationMethods")]
    RuleClass? Parent { get; }
}

[InterfaceOnly]
interface WithText
{
    string? Text { get; set; }
}

interface PropertyAnnotation : WithText
{
    [Container("Property.Annotations")]
    Property? Property { get; }
}

interface RuleAnnotation : WithText
{
    [Container("RuleClass.CustomRuleAnnotations")]
    RuleClass? Parent { get; }
}

interface TypeMappings
{
    [Contains("TypeMapping.Parent")]
    TypeMapping[] TypeMappings { get; }

    [Container("GrammarModel.TypeMappings")]
    GrammarModel? Model { get; }
}

interface TypeMapping
{
    [Container("TypeMappings.TypeMappings")]
    TypeMappings? Parent { get; }

    [Contains("Mapping.Parent")]
    Mapping[] Entries { get; }

    string[] ApplyToNames { get; }
}

interface Mapping
{
    [Container("TypeMapping.Entries")]
    TypeMapping? Parent { get; }

    string? RuleName { get; set; }
    string? TypeName { get; set; }
    string? TypeToStringCode { get; set; }
    string? StringToTypeCode { get; set; }
    string? DefaultValueCode { get; set; }
}
