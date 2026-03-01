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
        if (node.IsFlat())
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
            if (TAST.TryGetApplyRule(child.Id, out var inst))
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
                i += TAST.ToFlatSlice(inst.Span).Length;
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
                    Diagnostics.ReportUnexpected(new(token.Start, token.Length), null, "Unexpected Tokens");

                //SKIP
                i++;
            }
        }
    }
}