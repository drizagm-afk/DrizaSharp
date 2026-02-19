using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //MUTATE PROCESS
    private void MutateRule(ParserRuleInstance inst)
    => MutateRule(TAST.NodeAt(inst.NodeId), inst);
    private void EndMutate() => RuleInst = null;

    private void MutateRule(in TASTNode node, ParserRuleInstance inst)
    {
        RuleInst = inst;

        inst.Mutate(this);
        ApplyRewrite();

        MutateChildren(node);
    }
    private void MutateNode(in TASTNode node)
    {
        if (node.IsFlat())
        {
            EvalMatch(node.Id);
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
    private void EvalMatch(int nodeId, RuleId[] rules)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);

        //FIXED RULES
        int r = 0;

        //LOOP
        int i = 0;
        while (i < node.Length)
        {
            if (r >= rules.Length) break;

            InitMatch();
            ParserRuleInstance? inst;
            
            var ruleId = rules[r];
            if (ruleId.IsClass)
                inst = EvalRuleMatch(GetRuleClass(ruleId), new(nodeId, i, 0, -1));
            else
                inst = EvalRuleMatch(GetRule(ruleId), new(nodeId, i, 0, -1));

            //CLEAR
            ClearVarStorage();

            //EVAL
            Site.DropMemos();
            if (inst is not null)
            {
                //FINALIZATION
                i += inst.Span.Length;
                r++;

                //BUILD STRUCTURE
                BuildRule(inst);
                EndBuild();
            }
            else i++;
        }
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