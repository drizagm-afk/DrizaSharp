using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Rules;

internal partial class Ruleset
{
    //ARTIFACTS
    internal readonly Dictionary<Type, State> _states = [];

    //LEXER
    internal readonly List<TokenTypeData> _lTokentypes = [
        new(Tokens.NULL, false, false),
        new(Tokens.NEWLINE, false, false)
    ];
    internal readonly Dictionary<TokenTypeKey, int> _lTokentypesByKey = new()
    {
        [new(Tokens.NULL)] = 0,
        [new(Tokens.NEWLINE)] = 1
    };

    //PARSER
    internal readonly List<RealmData> _pRealms = [
        new(Realms.VIRTUAL)
    ];
    internal readonly Dictionary<RealmKey, int> _pRealmsByKey = new()
    {
        [new(Realms.VIRTUAL)] = 0
    };

    internal readonly List<Parser.RuleClass> _pClasses = [];

    internal readonly Dictionary<Type, Parser.RuleClass> _pClassesByType = [];
    internal readonly Dictionary<Type, Parser.Rule> _pRulesByType = [];

    //STRUCTURE
    internal readonly Dictionary<int, Dictionary<int, LocalRuleset>> _localRulesets = [];
}
internal partial class LocalRuleset
{
    //LEXER
    internal readonly List<Lexer.Rule> _lRules = [];

    internal readonly List<Lexer.PhaseHook> _lPhaseHooks = [];

    //PARSER
    internal readonly List<Parser.Rule> _pRules = [];
    internal readonly Dictionary<int, List<int>> _pRulesByRealm = [];
    internal readonly Dictionary<RuleId, List<(bool isClass, int id)>> _pRulesByClass = [];

    internal readonly List<Parser.PhaseHook> _pPhaseHooks = [];
    internal readonly Dictionary<RuleId, List<Parser.Hook>> _pHooks = [];
}