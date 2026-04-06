using System.Collections.Immutable;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    private void Match(int nodeId)
    => Match(TAST.NodeAt(nodeId));
    private void MatchInner(in TASTNode node)
    {
        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            if (!TAST.HasApplyRule(child.Id))
                Match(child);
            else
                MatchInner(child);

            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }
    private void Match(in TASTNode node)
    {
        var realm = TAST.InfoAt(node.Id).RealmId;

        //LOOP
        int i = 0;
        while (i < node.Length)
        {
            bool matched = false;
            foreach (var rule in Project.RulesPerRealm(Module, realm))
            {
                if (rule.IsAbstract) continue;

                //MATCH
                InitMatchMemory();
                var inst = MatchRule(rule, new(node.Id, i, 0, -1));

                ClearMatchMemory();

                //CHECK
                if (inst is not null)
                {
                    DropMemos();

                    //BUILD STRUCTURE
                    Build(inst);
                    MatchInner(TAST.NodeAt(inst.NodeId));

                    //NEXT
                    if (!TAST.InfoAt(node.Id).IsLinear)
                        TAST.UpdateLinearity(inst.NodeId);

                    i += TAST.ToFlatSlice(inst.Span).Length;
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
                if (Rules.Lexer.Ruleset.MustParse(Project, token.Type))
                    Diagnostics.ReportUnexpected(new(token.Start, token.Length), "Unexpected Tokens");

                //NEXT
                i++;
            }
        }
    }
    private void Match(in TASTNode node, ImmutableArray<RuleId> rules)
    {
        var realm = TAST.InfoAt(node.Id).RealmId;
        int ruleTry = 0;

        //LOOP
        int i = 0;
        while (i < node.Length && ruleTry < rules.Length)
        {
            //MATCH
            InitMatchMemory();

            TokenSpan span = new(node.Id, i, 0, -1);
            var ruleId = rules[ruleTry];

            RuleInstance? inst;
            if (ruleId.IsRuleClass())
                inst = MatchRuleClass(ruleId, span);
            else
                inst = MatchRule(Project.GetRule(ruleId), span);

            ClearMatchMemory();

            //CHECK
            DropMemos();
            if (inst is not null)
            {
                //BUILD STRUCTURE
                Build(inst);
                MatchInner(TAST.NodeAt(inst.NodeId));

                //NEXT
                if (!TAST.InfoAt(node.Id).IsLinear)
                    TAST.UpdateLinearity(inst.NodeId);

                i += TAST.ToFlatSlice(inst.Span).Length;
                ruleTry++;
            }
            else
            {
                //REPORT
                var token = TAST.TokenAt(node.Start + i);
                if (Rules.Lexer.Ruleset.MustParse(Project, token.Type))
                    Diagnostics.ReportUnexpected(new(token.Start, token.Length), "Unexpected Tokens");

                //NEXT
                i++;
            }
        }
    }
    private void InitMatchMemory()
    {
        _hash = 0;
        _lastHash = 0;

        _commitCode = -1;
        _lastCommit = -1;
        Commit();
    }
    private void ClearMatchMemory()
    {
        _matchVars.Clear();
        _matchVarDict.Clear();
        _matchRuleVarDict.Clear();
    }

    //>>>> MATCH PATTERN <<<<
    private int MatchPattern(Rule rule, TokenSpan span)
    {
        int len = rule.Pattern.Matches(this, span);
        return len <= 0 ? 0 : len;
    }
    private RuleInstance? GetRuleInstance(Rule rule, TokenSpan span)
    {
        var inst = rule.NewInstance();
        inst.Span = span;

        //INSTANTIATE
        void instRule(RuleBase rule)
        {
            if (rule.Parent is RuleClass parent)
                instRule(parent);

            rule.Instantiate(this, inst);
        }
        instRule(rule);
        return inst;
    }
    private RuleInstance? MatchRule(Rule rule, TokenSpan span)
    {
        //LOOK FOR MEMO
        if (TryGetMemo(rule, span, out var inst))
            return inst;

        //MANUAL MATCHING
        var len = MatchPattern(rule, span);

        if (len <= 0)
            inst = null;
        else
            inst = GetRuleInstance(rule, span.With(length: len));

        //SAVE MEMO
        SetMemo(rule, span, inst);
        return inst;
    }
    private RuleInstance? MatchRuleClass(RuleId clazzId, TokenSpan span)
    {
        foreach (var rule in Project.RulesPerClass(Module, clazzId))
            if (MatchRule(rule, span) is RuleInstance inst)
                return inst;

        return null;
    }

    //>>>> MATCH MEMOIZATION <<<<
    private readonly Dictionary<RuleMemoKey, RuleInstance?> _ruleMemoization = [];
    internal readonly record struct RuleMemoKey
    (RuleId RuleId, int Start);

    internal void SetMemo(Rule rule, TokenSpan span, RuleInstance? inst)
    => _ruleMemoization[new(rule.Id, span.Start)] = inst;
    internal bool TryGetMemo(Rule rule, TokenSpan span, out RuleInstance? inst)
    {
        if (_ruleMemoization.TryGetValue(new(rule.Id, span.Start), out inst))
            return true;

        //NESTING MEMOIZATION (RE-MATCH ONLY)
        if (TAST.TryGetNest(span, out var nestId)
        && TAST.TryGetApplyRule(nestId, out inst)
        && inst.RuleId.Equals(rule.Id))
        {
            inst.Span = span.With(length: inst.Span.Length);
            return true;
        }

        return false;
    }
    internal void DropMemos() => _ruleMemoization.Clear();
}