// Ported from eu.mihosoft.vmftest.complex.unparsermodel.vmfmodel.UnparserModel
// Shares the namespace (and therefore the VMF model) with GrammarLangModel.cs.

using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Complex.UnparserModel.VmfModel;

interface IUnparserModel
{
    [Contains("IUPRule.Parent")]
    IUPRule[] Rules { get; }

    [Contains("IUPLexerRule.Parent")]
    IUPLexerRule[] LexerRules { get; }
}

interface IUPLexerRule : IWithName, IWithText
{
    [Container("IUnparserModel.LexerRules")]
    IUnparserModel? Parent { get; }
}

[InterfaceOnly]
interface IWithTokenLocation
{
    [IgnoreEquals] int TokenIndexStart { get; set; }
    [IgnoreEquals] int TokenIndexStop { get; set; }
}

[InterfaceOnly]
interface IWithAltId
{
    int AltId { get; set; }
}

[InterfaceOnly]
interface IWithElementId
{
    int ElementId { get; set; }
}

[InterfaceOnly]
interface IWithRuleId
{
    int RuleId { get; set; }
}

[InterfaceOnly]
interface IUPRuleBase : IWithRuleId
{
    [Contains("IAlternativeBase.ParentRule")]
    IAlternativeBase[] Alternatives { get; }
}

interface IUPRule : IWithName, IUPRuleBase, IWithTokenLocation
{
    [Container("IUnparserModel.Rules")]
    IUnparserModel? Parent { get; }

    [IgnoreEquals] int TokenIndexCOLON { get; set; }
    [IgnoreEquals] int TokenIndexLOCALS { get; set; }
}

[InterfaceOnly]
interface ISubRule : IUPRuleBase
{
}

[InterfaceOnly]
interface IAlternativeBase : IWithText, IWithAltId
{
    [Container("IUPRuleBase.Alternatives")]
    IUPRuleBase? ParentRule { get; }

    [Contains("IUPElement.ParentAlt")]
    IUPElement[] Elements { get; }
}

interface IUPElement : IWithText, IWithElementId, IWithTokenLocation
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

interface IUPNamedElement : IUPElement, IWithName
{
}

interface IUPSubRuleElement : IUPElement, ISubRule
{
}

interface IUPNamedSubRuleElement : IUPElement, ISubRule, IWithName
{
}

interface IAlternative : IAlternativeBase
{
}

interface ILabeledAlternative : IAlternative, IWithName
{
}
