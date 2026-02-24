using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== EXECUTE EMIT =====
    public void Emit(ParserSite site)
    {
        Site = site;

        if (TAST._outNodes.TryGetValue(site.RootId, out var emitId))
            Emit(site.RootId, emitId);
        else
            Emit(site.RootId, new());
    }
    private void Emit(int nodeId, EmitId emitId)
    => Emit(TAST.NodeAt(nodeId), emitId);
    private void Emit(in TASTNode node, EmitId emitId)
    {
        if (node.Args.OutCode != PhaseCode)
        {
            TAST._outNodes[node.Id] = emitId;
            return;
        }

        //NON-RULE EMIT
        if (!Site._ruleAppliance.TryGetValue(node.Id, out var inst) || inst.IsRewritten)
        {
            var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
            while (childExists)
            {
                Emit(child, emitId);
                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
            }
            return;
        }

        //RULE EMIT
        RuleInst = inst;

        _instructCount = TASI.InstructionCount;
        inst.EmitId = emitId;
        inst.Emit(this);
    }
}