using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== EXECUTE MATCHING =====
    public void EvalMatch(ParserSite site)
    {
        Site = site;
        EvalMatch(site.RootId);
    }
    private void EvalMatch(int nodeId)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);

        //LOOP
        int i = 0;
        while (i < node.Length)
        {
            int matched = 0;
            for (int r = ParserManager._rules.Count - 1; r >= 0; r--)
            {
                var rule = ParserManager._rules[r];

                if (rule.IsAbstract) continue;
                if (!rule.Evals(new(_phaseCode, node.Args.RealmCode))) continue;

                //TRY MATCH
                InitMatch();
                var inst = EvalRuleMatch(rule, new(nodeId, i, 0, node.Length - i));

                //CLEAR
                ClearVarStorage();

                //EVAL
                if (inst is not null)
                {
                    //FINALIZATION
                    Site.DropMemos();
                    matched = inst.Span.Length;

                    //BUILD STRUCTURE
                    StartBuild(inst);
                    EndBuild();

                    //MUTATE STRUCTURE
                    StartMutate(inst);
                    EndMutate();

                    //END
                    break;
                }
                else continue;
            }

            if (matched > 0) i += matched;
            else
            {
                Site.DropMemos();
                i++;
            }
        }
    }

    //MATCH
    private bool TryGetMemo(Rule rule, TokenSpan span, [NotNullWhen(true)] out RuleInstance? inst)
    {
        //SITE MEMOIZATION CHECK
        if (Site.TryGetRuleMemo(span.Start, rule.Id, out inst))
            return true;

        //NESTING MEMOIZATION CHECK (REWRITE ONLY)
        if (TryFindNestAtSpan(span, out var nestId)
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
        Debug.Assert(IsNestValidAtSpan(inst.Span, out _, out _));

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
    private RuleInstance? EvalRuleMatch(Rule rule, TokenSpan span)
    {
        //CHECK FOR MEMO
        if (TryGetMemo(rule, span, out var inst))
            return inst;

        //MANUAL EVAL
        inst = TryMatchRule(rule, span);
        Site.SetRuleMemo(span.Start, rule.Id, inst);

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
    private RuleInstance? EvalRuleMatch(RuleClass rule, TokenSpan span)
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
            if (subRule.IsClass)
                inst = TryMatchRuleClass(length, GetRuleClass(subRule), span);
            else
                inst = TryMatchRuleFromClass(length, GetRule(subRule), span);

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

public partial class ParserSite
{
    private readonly Dictionary<RuleMemoKey, RuleInstance?> _ruleMemoization = [];
    internal void SetRuleMemo(int start, RuleId ruleId, RuleInstance? inst)
    {
        RuleMemoKey key = new(start, ruleId);
        _ruleMemoization[key] = inst;
    }
    internal bool TryGetRuleMemo(int start, RuleId ruleId, [NotNullWhen(true)] out RuleInstance? inst)
    {
        inst = null;
        if (!_ruleMemoization.TryGetValue(new(start, ruleId), out var eval))
            return false;

        inst = eval!;
        return true;
    }
    internal void DropMemos() => _ruleMemoization.Clear();
}

internal readonly record struct RuleMemoKey
(int Start, RuleId RuleId);