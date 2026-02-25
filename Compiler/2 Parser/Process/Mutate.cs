using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //MUTATE PROCESS
    private void StartMutate(RuleInstance inst)
    => MutateRule(TAST.NodeAt(inst.NodeId), inst);
    private void EndMutate() => RuleInst = null;

    private void MutateRule(in TASTNode node, RuleInstance inst)
    {
        RuleInst = inst;

        inst.Mutate(this);
        ApplyRewrite();

        MutateChildren(node);
    }
    private void MutateNode(in TASTNode node)
    {
        if (TAST.ArgsAt(node.Id).OutCode == _phaseCode && node.IsFlat())
        {
            Match(node.Id);
            return;
        }

        MutateChildren(node);
    }
    private void MutateChildren(in TASTNode node)
    {
        if (node.IsFlat()) return;

        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            if (Site._ruleAppliance.TryGetValue(child.Id, out var inst))
                MutateRule(child, inst);
            else
                MutateNode(child);

            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }

    //REWRITING EVAL
    private void Match(int nodeId, int[] rules)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);

        //FIXED RULES
        int r = 0;

        //LOOP
        int i = 0;
        while (i < node.Length && r < rules.Length)
        {
            InitMatch();
            RuleInstance? inst;

            var rinfo = ParserManager.GetRuleInfo(rules[r]);
            if (rinfo.IsClass)
                inst = MatchRule(ParserManager.GetRuleClass(rinfo), new(nodeId, i, 0, -1));
            else
                inst = MatchRule(ParserManager.GetRule(rinfo), new(nodeId, i, 0, -1));

            //CLEAR
            ClearVarStorage();

            //EVAL
            DropMemos();
            if (inst is not null)
            {
                //FINALIZATION
                i += FlatLength(inst.Span);
                r++;

                //BUILD STRUCTURE
                StartBuild(inst);
                EndBuild();
            }
            else
            {
                //REPORT
                var token = TAST.TokenAt(node.Start + i);
                if (token.Type != Token.NEWLINE)
                    Diagnostics.ReportUnexpected(new(token.Start, token.Length));

                //SKIP
                i++;
            }
        }
    }
    private int FlatLength(TokenSpan span)
    {
        ref readonly var node = ref TAST.NodeAt(span.NodeId);

        int count = 0;
        int i = TAST.SkipOffset(node, span.Offset, out var childExists, out var child);
        while (i < node.Length && count < span.Length)
        {
            if (childExists && child.RelStart == i)
            {
                i += child.RelLength;
                count += child.Length;

                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                continue;
            }

            i++;
            count++;
        }

        return i - span.Offset;
    }

    //SAFE NESTING CHECKOUT
    private bool TryFindNestAtSpan(TokenSpan span, out int nestId)
    {
        nestId = 0;
        ref readonly var node = ref TAST.NodeAt(span.NodeId);

        int i = TAST.SkipOffset(node, span.Offset, out var childExists, out var child);
        int order = span.Start;
        while (i < node.Length)
        {
            if (!childExists) return false;
            else if (child.RelStart == i)
            {
                if (order <= 0)
                {
                    nestId = child.Id;
                    return true;
                }
                order -= child.Length;
                i += child.RelLength;

                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                continue;
            }

            if (order <= 0) return false;
            order--;
            i++;
        }
        return false;
    }
    private bool IsNestValidAtSpan(TokenSpan span, out bool hasNest, out int nestId)
    {
        hasNest = false;
        nestId = 0;
        if (span.Length <= 0) return false;

        ref readonly var node = ref TAST.NodeAt(span.NodeId);

        int i = TAST.SkipOffset(node, span.Offset, out var childExists, out var child); ;
        int order = span.Start;
        while (i < node.Length)
        {
            if (!childExists) return true;
            else if (child.RelStart == i)
            {
                if (order <= 0)
                {
                    nestId = child.Id;
                    hasNest = child.Length == span.Length;
                    return child.Length <= span.Length;
                }
                order -= child.Length;
                i += child.RelLength;

                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                continue;
            }

            if (order <= 0)
            {
                int rem = span.Length + order;
                while (childExists)
                {
                    rem -= child.RelStart - i;
                    if (rem <= 0) return true;

                    rem -= child.Length;
                    i += child.RelLength;
                    if (rem < 0) return false;
                    else if (rem == 0) return true;

                    childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                }
                return true;
            }
            order--;
            i++;
        }
        return false;
    }
}