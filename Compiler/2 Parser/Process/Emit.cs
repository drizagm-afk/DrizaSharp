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
        if (!TAST.TryGetApplyRule(node.Id, out var inst) || inst.BypassEmit)
        {
            DefaultEmit(node, emitId);
            return;
        }

        //RULE EMIT
        _instructCount = TASI.InstructionCount;
        _dataCount = TASI.DataCount;

        RuleInst = inst;
        inst.EmitId = emitId;

        var count = TASI.NodeCount;
        inst.Emit(this);

        if (count == TASI.NodeCount)
            Diagnostics.ReportInvalid(
                TAST.SourceSlice(inst.NodeId), null,
                $"{ParserManager.GetRuleName(inst.RuleId)} doesn't EMIT, if intentional, set BypassEmit = true"
            );
    }
    private void DefaultEmit(in TASTNode node, TASTEmit emitId)
    {
        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            Emit(child, emitId);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }
}