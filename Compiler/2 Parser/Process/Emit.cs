using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== EXECUTE EMIT =====
    public void Emit(DzFile file)
    {
        File = file;
        Emit(TAST.Root, new());
    }
    private void Emit(int nodeId, TASTEmit emitId)
    => Emit(TAST.NodeAt(nodeId), emitId);
    private void Emit(in TASTNode node, TASTEmit emitId)
    {
        //NON-RULE EMIT
        if (!TAST.TryGetApplyRule(node.Id, out var inst) || TAST.InfoAt(node.Id).IsRewritten)
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