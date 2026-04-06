using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Rules.Lexer;

internal static class RuleExt
{
    //>>>> TOKEN TYPES <<<<
    public static TokenTypeData TokenTypeAt(this DzProject proj, int id)
    => proj.Ruleset()._lTokentypes[id];
    public static int TokenTypeId(this DzProject proj, string tokenName)
    => proj.Ruleset()._lTokentypesByKey[new(tokenName)];
    public static TokenTypeData TokenType(this DzProject proj, string tokenName)
    => proj.TokenTypeAt(proj.TokenTypeId(tokenName));

    //>>>> RULES <<<<
    public static Rule GetRule(this DzProject proj, RuleId id)
    => proj.Ruleset(id)._lRules[id.LocalId];

    //MATCHING
    public static IEnumerable<Rule> Rules(this DzProject proj, DzModule module)
    {
        var deps = module.Dependencies;
        for (int i = deps.Length - 1; i >= 0; i--)
        {
            var dep = deps[i];

            var rules = proj.Ruleset(dep)._lRules;
            for (int j = rules.Count - 1; j >= 0; j--)
                yield return rules[j];
        }
    }
}