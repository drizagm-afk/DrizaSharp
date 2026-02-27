using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //==== MATCHING MEMOIZATION =====
    private readonly Dictionary<RuleMemoKey, RuleInstance?> _ruleMemoization = [];
    internal void SetRuleMemo(int start, int ruleId, RuleInstance? inst)
    {
        RuleMemoKey key = new(start, ruleId);
        _ruleMemoization[key] = inst;
    }
    internal bool TryGetRuleMemo(int start, int ruleId, [NotNullWhen(true)] out RuleInstance? inst)
    {
        inst = null;
        if (!_ruleMemoization.TryGetValue(new(start, ruleId), out var eval))
            return false;

        inst = eval!;
        return true;
    }
    internal void DropMemos() => _ruleMemoization.Clear();

    //===== EXECUTE MATCHING =====
    public void Match(ParserSite site)
    {
        Site = site;
        Match(site.RootId);
    }
    private void Match(int nodeId)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);

        //LOOP
        int i = 0;
        while (i < node.Length)
        {
            bool matched = false;
            for (int r = ParserManager._rules.Count - 1; r >= 0; r--)
            {
                var rule = ParserManager._rules[r];

                if (rule.IsAbstract) continue;
                if (!rule.Evals(TAST.ArgsAt(node.Id).RealmId)) continue;

                //TRY MATCH
                InitMatch();
                var inst = MatchRule(rule, new(nodeId, i, 0, -1));

                //CLEAR
                ClearVarStorage();

                //EVAL
                if (inst is not null)
                {
                    //FINALIZATION
                    DropMemos();

                    //BUILD STRUCTURE
                    StartBuild(inst);
                    EndBuild();

                    //MUTATE STRUCTURE
                    StartMutate(inst);
                    EndMutate();

                    //END
                    i += inst.Span.Length;
                    matched = true;
                    break;
                }
                else continue;
            }

            if (!matched)
            {
                DropMemos();

                //REPORT
                var token = TAST.TokenAt(node.Start + i);
                if (token.Type != Token.NEWLINE)
                    Diagnostics.ReportUnexpected(new(token.Start, token.Length), null, "Unexpected Tokens");

                //SKIP
                i++;
            }
        }
    }

    //MATCH
    private bool TryGetMemo(Rule rule, TokenSpan span, [NotNullWhen(true)] out RuleInstance? inst)
    {
        //SITE MEMOIZATION CHECK
        if (TryGetRuleMemo(span.Start, rule.Id, out inst))
            return true;

        //NESTING MEMOIZATION CHECK (REWRITE ONLY)
        if (TAST.TryGetNest(span, out var nestId)
        && Site._ruleAppliance.TryGetValue(nestId, out inst)
        && inst.RuleId.Equals(rule.Id))
        {
            inst.Span = span.With(length: inst.Span.Length);
            return true;
        }

        return false;
    }
    private int TryMatchPatterns(int length, RuleBase rule, TokenSpan span)
    {
        foreach (var pattern in rule.Patterns)
        {
            if (pattern._varName is null)
            {
                Debug.Assert(length <= 0, $"MORE THAN ONE NON-VAR PATTERNS AREN'T ALLOWED: rule={rule.GetType().Name}");
                length = pattern.Matches(this, span);
                if (length <= 0) return 0;
            }
            else if (!EvalVars(pattern._varName, pattern)) return 0;
        }
        return length > 0 ? length : 0;
    }
    private RuleInstance? GetRuleInstance(Rule rule, TokenSpan span)
    {
        var inst = rule.NewInstance();
        inst.Span = span;

        //INSTANTIATE
        void instRule(RuleBase rule)
        {
            var parent = rule.Parent;
            if (parent is not null)
                instRule(parent);

            rule.Instantiate(this, inst);
        }
        instRule(rule);
        return inst;
    }

    //RULE MATCH
    private RuleInstance? MatchRule(Rule rule, TokenSpan span)
    {
        //CHECK FOR MEMO
        if (TryGetMemo(rule, span, out var inst))
            return inst;

        //MANUAL EVAL
        inst = TryMatchRule(rule, span);
        SetRuleMemo(span.Start, rule.Id, inst);

        return inst;
    }
    private RuleInstance? TryMatchRule(Rule rule, TokenSpan span)
    {
        //MATCH PATTERN
        var length = 0;
        bool matchRule(RuleBase rule)
        {
            var parent = rule.Parent;
            if (parent is not null)
            {
                if (!matchRule(parent)) return false;
            }

            if (rule.Patterns.Length > 0)
            {
                length = TryMatchPatterns(length, rule, span);
                if (length <= 0) return false;
            }
            return true;
        }
        if (!matchRule(rule) || length <= 0) return null;

        //INSTANTIATE
        return GetRuleInstance(rule, span.With(length: length));
    }

    //RULE CLASS MATCH
    private RuleInstance? MatchRule(RuleClass rule, TokenSpan span)
    {
        int length = 0;
        bool matchRule(RuleBase? rule)
        {
            if (rule is not null)
            {
                if (!matchRule(rule.Parent)) return false;
                if (rule.Patterns.Length > 0)
                {
                    length = TryMatchPatterns(length, rule, span);
                    if (length <= 0) return false;
                }
            }
            return true;
        }
        if (!matchRule(rule.Parent)) return null;

        return TryMatchRuleClass(length, rule, span);
    }
    private RuleInstance? TryMatchRuleClass(int length, RuleClass rule, TokenSpan span)
    {
        //MATCH PATTERN
        if (rule.Patterns.Length > 0)
        {
            length = TryMatchPatterns(length, rule, span);
            if (length <= 0) return null;
        }

        //EVAL SUBRULES
        foreach (var subRule in rule.SubRules)
        {
            var com = Commit();

            //TRY MATCH SUBRULE
            RuleInstance? inst;
            var rinfo = ParserManager.GetRuleInfo(subRule);
            if (rinfo.IsClass)
                inst = TryMatchRuleClass(length, ParserManager.GetRuleClass(rinfo), span);
            else
                inst = TryMatchRuleFromClass(length, ParserManager.GetRule(rinfo), span);

            if (inst is not null)
                return inst;

            //ROLLBACK IF NOT MATCH
            Rollback(com);
        }
        return null;
    }
    private RuleInstance? TryMatchRuleFromClass(int length, Rule rule, TokenSpan span)
    {
        //CHECK FOR MEMO
        if (TryGetMemo(rule, span, out var inst))
            return inst;

        //MATCH PATTERN
        if (rule.Patterns.Length > 0)
        {
            length = TryMatchPatterns(length, rule, span);
        }
        if (length <= 0) return null;

        //INSTANTIATE
        return GetRuleInstance(rule, span.With(length: length));
    }
}
internal readonly record struct RuleMemoKey
(int Start, int RuleId);