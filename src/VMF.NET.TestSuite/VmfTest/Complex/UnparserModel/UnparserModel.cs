// Ported from eu.mihosoft.vmftest.complex.unparsermodel.vmfmodel.UnparserModel
// Shares the namespace (and therefore the VMF model) with GrammarLangModel.cs.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel;

[VmfModel]
public partial interface IUnparserModel
{
    [Contains("IUPRule.Parent")]
    VList<IUPRule> Rules { get; }

    [Contains("IUPLexerRule.Parent")]
    VList<IUPLexerRule> LexerRules { get; }
}

[VmfModel]
public partial interface IUPLexerRule : IWithName, IWithText
{
    [Container("IUnparserModel.LexerRules")]
    IUnparserModel? Parent { get; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithTokenLocation
{
    [IgnoreEquals] int TokenIndexStart { get; set; }
    [IgnoreEquals] int TokenIndexStop { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithAltId
{
    int AltId { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithElementId
{
    int ElementId { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IWithRuleId
{
    int RuleId { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface IUPRuleBase : IWithRuleId
{
    [Contains("IAlternativeBase.ParentRule")]
    VList<IAlternativeBase> Alternatives { get; }
}

[VmfModel]
public partial interface IUPRule : IWithName, IUPRuleBase, IWithTokenLocation
{
    [Container("IUnparserModel.Rules")]
    IUnparserModel? Parent { get; }

    [IgnoreEquals] int TokenIndexCOLON { get; set; }
    [IgnoreEquals] int TokenIndexLOCALS { get; set; }
}

[VmfModel]
[InterfaceOnly]
public partial interface ISubRule : IUPRuleBase
{
}

[VmfModel]
[InterfaceOnly]
public partial interface IAlternativeBase : IWithText, IWithAltId
{
    [Container("IUPRuleBase.Alternatives")]
    IUPRuleBase? ParentRule { get; }

    [Contains("IUPElement.ParentAlt")]
    VList<IUPElement> Elements { get; }
}

[VmfModel]
public partial interface IUPElement : IWithText, IWithElementId, IWithTokenLocation
{
    // Settable so containment can be driven from the child side, as the Java fact does.
    // Java generates a container setter automatically; in VMF.NET the model interface IS
    // the public API, so `set` is how a model opts in.
    [Container("IAlternativeBase.Elements")]
    IAlternativeBase? ParentAlt { get; set; }

    bool ListType { get; set; }
    bool LexerRule { get; set; }
    bool Terminal { get; set; }
    bool ParserRule { get; set; }
    bool Action { get; set; }
    bool Negated { get; set; }
    string? RuleName { get; set; }
}

[VmfModel]
public partial interface IUPNamedElement : IUPElement, IWithName
{
}

[VmfModel]
public partial interface IUPSubRuleElement : IUPElement, ISubRule
{
}

[VmfModel]
public partial interface IUPNamedSubRuleElement : IUPElement, ISubRule, IWithName
{
}

[VmfModel]
public partial interface IAlternative : IAlternativeBase
{
}

[VmfModel]
public partial interface ILabeledAlternative : IAlternative, IWithName
{
}
