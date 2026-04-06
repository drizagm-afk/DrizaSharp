using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> EMIT PROJECT <<<<
    public partial void Emit()
    {
        foreach (var file in Project.Files)
            Emit(file);
    }

    //>>>> EMIT FILE <<<<
    public void Emit(DzFile file)
    {
        File = file;
        Emit(TAST.Root, default);
    }
    private void Emit(in TASTNode node, EmitTarget target)
    {
        //AUTO EMIT
        if (!TAST.TryGetApplyRule(node.Id, out var inst) || inst.BypassEmit)
        {
            var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
            while (childExists)
            {
                Emit(child, target);
                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
            }
            return;
        }

        //MANUAL EMIT
        _innerEmits.Clear();
        _instrCount = TASI.InstructionCount;
        _dataCount = TASI.DataCount;

        RuleInst = inst;
        inst.EmitTarget = target;

        var count = TASI.NodeCount;
        inst.Emit(this);

        if (count == TASI.NodeCount)
            Diagnostics.ReportUnhandled(
                TAST.SourceSlice(inst.NodeId), inst.NodeId,
                "The node doesn't Emit any emit-node, if intentional, set BypassEmit = true"
            );
    }
}