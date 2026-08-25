// Ported from eu.mihosoft.vmftest.complex.unparsermodel.vmfmodel.UnparserModel
// Shares the namespace (and therefore the VMF model) with GrammarLangModel.cs.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel.VmfModel;

interface UnparserModel
{
    [Contains("UPRule.Parent")]
    UPRule[] Rules { get; }

    [Contains("UPLexerRule.Parent")]
    UPLexerRule[] LexerRules { get; }
}

interface UPLexerRule : WithName, WithText
{
    [Container("UnparserModel.LexerRules")]
    UnparserModel? Parent { get; }
}

[InterfaceOnly]
interface WithTokenLocation
{
    [IgnoreEquals] int TokenIndexStart { get; set; }
    [IgnoreEquals] int TokenIndexStop { get; set; }
}

[InterfaceOnly]
interface WithAltId
{
    int AltId { get; set; }
}

[InterfaceOnly]
interface WithElementId
{
    int ElementId { get; set; }
}

[InterfaceOnly]
interface WithRuleId
{
    int RuleId { get; set; }
}

[InterfaceOnly]
interface UPRuleBase : WithRuleId
{
    [Contains("AlternativeBase.ParentRule")]
    AlternativeBase[] Alternatives { get; }
}

interface UPRule : WithName, UPRuleBase, WithTokenLocation
{
    [Container("UnparserModel.Rules")]
    UnparserModel? Parent { get; }

    [IgnoreEquals] int TokenIndexCOLON { get; set; }
    [IgnoreEquals] int TokenIndexLOCALS { get; set; }
}

[InterfaceOnly]
interface SubRule : UPRuleBase
{
}

[InterfaceOnly]
interface AlternativeBase : WithText, WithAltId
{
    [Container("UPRuleBase.Alternatives")]
    UPRuleBase? ParentRule { get; }

    [Contains("UPElement.ParentAlt")]
    UPElement[] Elements { get; }
}

interface UPElement : WithText, WithElementId, WithTokenLocation
{
    // Settable so containment can be driven from the child side, as the Java fact does.
    // Java generates a container setter automatically; in VMF.NET the model interface S
    // the public API, so `set` is how a model opts in.
    [Container("AlternativeBase.Elements")]
    AlternativeBase? ParentAlt { get; set; }

    bool ListType { get; set; }
    bool LexerRule { get; set; }
    bool Terminal { get; set; }
    bool ParserRule { get; set; }
    bool Action { get; set; }
    bool Negated { get; set; }
    string? RuleName { get; set; }
}

interface UPNamedElement : UPElement, WithName
{
}

interface UPSubRuleElement : UPElement, SubRule
{
}

interface UPNamedSubRuleElement : UPElement, SubRule, WithName
{
}

interface Alternative : AlternativeBase
{
}

interface LabeledAlternative : Alternative, WithName
{
}
