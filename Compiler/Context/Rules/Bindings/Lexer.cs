using DrzSharp.Compiler.Rules.Lexer;

namespace DrzSharp.Compiler.Rules;

public interface LexerBinding : ArtifactsBinding
{
    //>>>> MODEL <<<<
    public int AddTokenType(string tokenName, bool showValue = true, bool mustParse = true);
    public int TokenType(string tokenName);

    //>>>> RULES <<<<
    public RuleId BindRule(Rule rule);

    //>>>> HOOKS <<<<
    public void HookPhase<H>() where H : PhaseHook, new();
}

internal partial class RulesetBinding : LexerBinding
{
    //>>>> MODEL <<<<
    int LexerBinding.AddTokenType(string tokenName, bool showValue, bool mustParse)
    {
        var tokens = _ruleset._lTokentypes;

        int id = tokens.Count;
        tokens.Add(new(tokenName, showValue, mustParse));

        return id;
    }
    int LexerBinding.TokenType(string tokenName)
    => _ruleset._lTokentypesByKey[new(tokenName)];

    //>>>> RULES <<<<
    RuleId LexerBinding.BindRule(Rule rule)
    {
        var rules = _localRuleset._lRules;

        int id = rules.Count;
        rules.Add(rule);

        return NewRuleId(id);
    }

    //>>>> HOOKS <<<<
    void LexerBinding.HookPhase<H>()
    {
        var hooks = _localRuleset._lPhaseHooks;

        hooks.Add(new H());
    }
}