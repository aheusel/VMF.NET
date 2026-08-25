// Ported from eu.mihosoft.vmftest.complex.vmf_text.grammarmodel.vmfmodel.GrammarLangModel
//
// Near-identical to the unparsermodel copy, but: no model-wide equality setting, and
// SuperClass/ChildClasses form a CONTAINMENT pair here (not cross-references).

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.VmfText.GrammarModel.VmfModel;

interface IGrammarModel
{
    [Contains("IRuleClass.Model")]
    VList<IRuleClass> RuleClasses { get; }

    string? GrammarName { get; set; }
    string? PackageName { get; set; }

    [Contains("ITypeMappings.Model")]
    ITypeMappings? TypeMappings { get; set; }

    [Contains("ICustomRule.Model")]
    VList<ICustomRule> CustomRules { get; }
}

interface ICustomRule : IWithText
{
    [Container("IGrammarModel.CustomRules")]
    IGrammarModel? Model { get; }
}

[Immutable]
interface ICodeRange
{
    ICodeLocation? Start { get; }
    ICodeLocation? Stop { get; }
    int Length { get; }
}

[Immutable]
interface ICodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[InterfaceOnly]
interface ILangElement
{
}

[InterfaceOnly]
interface ICodeElement
{
    ICodeRange? CodeRange { get; set; }
}

[InterfaceOnly]
interface IWithType : ILangElement
{
    IType? Type { get; set; }
}

[InterfaceOnly]
interface IWithName : ILangElement
{
    [GetterOnly]
    string? Name { get; }
}

[Immutable]
interface IType : ILangElement, IWithName
{
    string? PackageName { get; }
    string? AntlrRuleName { get; }
    bool RuleType { get; }
    bool ArrayType { get; }
}

interface IRuleClass : IWithName, ICodeElement
{
    [Container("IGrammarModel.RuleClasses")]
    IGrammarModel? Model { get; }

    [Contains("IProperty.Parent")]
    VList<IProperty> Properties { get; }

    [Contains("IProperty.Parent")]
    VList<IProperty> CustomProperties { get; }

    [Container("IRuleClass.ChildClasses")]
    IRuleClass? SuperClass { get; }

    VList<string> SuperInterfaces { get; }

    [Contains("IRuleClass.SuperClass")]
    VList<IRuleClass> ChildClasses { get; }

    bool Root { get; set; }

    [Contains("IDelegationMethod.Parent")]
    VList<IDelegationMethod> DelegationMethods { get; }

    [Contains("IRuleAnnotation.Parent")]
    VList<IRuleAnnotation> CustomRuleAnnotations { get; }
}

interface IProperty : IWithName, IWithType, ICodeElement
{
    [Container("IRuleClass.Properties")]
    IRuleClass? Parent { get; }

    [Contains("IPropertyAnnotation.Property")]
    VList<IPropertyAnnotation> Annotations { get; }
}

interface IDelegationMethod : IWithText
{
    [Container("IRuleClass.DelegationMethods")]
    IRuleClass? Parent { get; }
}

[InterfaceOnly]
interface IWithText
{
    string? Text { get; set; }
}

interface IPropertyAnnotation : IWithText
{
    [Container("IProperty.Annotations")]
    IProperty? Property { get; }
}

interface IRuleAnnotation : IWithText
{
    [Container("IRuleClass.CustomRuleAnnotations")]
    IRuleClass? Parent { get; }
}

interface ITypeMappings
{
    [Contains("ITypeMapping.Parent")]
    VList<ITypeMapping> TypeMappings { get; }

    [Container("IGrammarModel.TypeMappings")]
    IGrammarModel? Model { get; }
}

interface ITypeMapping
{
    [Container("ITypeMappings.TypeMappings")]
    ITypeMappings? Parent { get; }

    [Contains("IMapping.Parent")]
    VList<IMapping> Entries { get; }

    VList<string> ApplyToNames { get; }
}

interface IMapping
{
    [Container("ITypeMapping.Entries")]
    ITypeMapping? Parent { get; }

    string? RuleName { get; set; }
    string? TypeName { get; set; }
    string? TypeToStringCode { get; set; }
    string? StringToTypeCode { get; set; }
    string? DefaultValueCode { get; set; }
}
