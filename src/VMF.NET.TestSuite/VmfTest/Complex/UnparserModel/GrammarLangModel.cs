// Ported from eu.mihosoft.vmftest.complex.unparsermodel.vmfmodel.GrammarLangModel
// (same Java package as UnparserModel.java -> same C# namespace / same VMF model)

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel;

[VmfModel(Equality = EqualsType.ContainmentAndExternal)]
public partial interface IGrammarModel
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

[VmfModel]
public partial interface ICustomRule : IWithText
{
    [Container("IGrammarModel.CustomRules")]
    IGrammarModel? Model { get; }
}

[VmfModel]
[Immutable]
public partial interface ICodeRange
{
    ICodeLocation? Start { get; }
    ICodeLocation? Stop { get; }
    int Length { get; }
}

[VmfModel]
[Immutable]
public partial interface ICodeLocation
{
    int Index { get; }
    int Line { get; }
    int CharPosInLine { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface ILangElement
{
}

[VmfModel]
[InterfaceOnly]
public partial interface ICodeElement
{
    ICodeRange? CodeRange { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithType : ILangElement
{
    IType? Type { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithName : ILangElement
{
    [GetterOnly]
    string? Name { get; }
}

[VmfModel]
[Immutable]
public partial interface IType : ILangElement, IWithName
{
    string? PackageName { get; }
    string? AntlrRuleName { get; }
    bool RuleType { get; }
    bool ArrayType { get; }
}

[VmfModel]
public partial interface IRuleClass : IWithName, ICodeElement
{
    [Container("IGrammarModel.RuleClasses")]
    IGrammarModel? Model { get; }

    [Contains("IProperty.Parent")]
    VList<IProperty> Properties { get; }

    [Contains("IProperty.Parent")]
    VList<IProperty> CustomProperties { get; }

    [Refers("IRuleClass.ChildClasses")]
    IRuleClass? SuperClass { get; set; }

    VList<string> SuperInterfaces { get; }

    [Refers("IRuleClass.SuperClass")]
    VList<IRuleClass> ChildClasses { get; }

    bool Root { get; set; }

    [Contains("IDelegationMethod.Parent")]
    VList<IDelegationMethod> DelegationMethods { get; }

    [Contains("IRuleAnnotation.Parent")]
    VList<IRuleAnnotation> CustomRuleAnnotations { get; }
}

[VmfModel]
public partial interface IProperty : IWithName, IWithType, ICodeElement
{
    // several properties on IRuleClass contain IProperty -> no single opposite
    [Container]
    IRuleClass? Parent { get; }

    [Contains("IPropertyAnnotation.Property")]
    VList<IPropertyAnnotation> Annotations { get; }
}

[VmfModel]
public partial interface IDelegationMethod : IWithText
{
    [Container("IRuleClass.DelegationMethods")]
    IRuleClass? Parent { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithText
{
    string? Text { get; set; }
}

[VmfModel]
public partial interface IPropertyAnnotation : IWithText
{
    [Container("IProperty.Annotations")]
    IProperty? Property { get; }
}

[VmfModel]
public partial interface IRuleAnnotation : IWithText
{
    [Container("IRuleClass.CustomRuleAnnotations")]
    IRuleClass? Parent { get; }
}

[VmfModel]
public partial interface ITypeMappings
{
    [Contains("ITypeMapping.Parent")]
    VList<ITypeMapping> TypeMappings { get; }

    [Container("IGrammarModel.TypeMappings")]
    IGrammarModel? Model { get; }
}

[VmfModel]
public partial interface ITypeMapping
{
    [Container("ITypeMappings.TypeMappings")]
    ITypeMappings? Parent { get; }

    [Contains("IMapping.Parent")]
    VList<IMapping> Entries { get; }

    VList<string> ApplyToNames { get; }
}

[VmfModel]
public partial interface IMapping
{
    [Container("ITypeMapping.Entries")]
    ITypeMapping? Parent { get; }

    string? RuleName { get; set; }
    string? TypeName { get; set; }
    string? TypeToStringCode { get; set; }
    string? StringToTypeCode { get; set; }
    string? DefaultValueCode { get; set; }
}
