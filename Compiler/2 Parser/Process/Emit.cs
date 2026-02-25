using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== EXECUTE EMIT =====
    public void Emit(ParserSite site)
    {
        Site = site;

        if (site.RootId == TAST.RootId)
            Emit(site.RootId, new());
        else
            Emit(site.RootId, TAST.InfoAt(site.RootId).EmitId);
    }
    private void Emit(int nodeId, TASTEmit emitId)
    => Emit(TAST.NodeAt(nodeId), emitId);
    private void Emit(in TASTNode node, TASTEmit emitId)
    {
        if (TAST.ArgsAt(node.Id).OutCode != PhaseCode)
        {
            TAST.UpdateInfo(node.Id, emitId: emitId);
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