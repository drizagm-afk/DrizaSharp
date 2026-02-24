using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface EmitContext
{
    public void Emit(EmitId emitId = new(), params EmitNode[] emitNodes);
}

public partial class ParserProcess : EmitContext
{
    private int _instructCount = 0;
    public void Emit(EmitId emitId = new(), params EmitNode[] emitNodes)
    {
        if (!emitId.IsValid) emitId = RuleInst!.EmitId;

        //EMIT
        var count = TASI.InstructionCount;
        if (_instructCount == count)
            throw new Exception("CANNOT EMIT WITH ZERO INSTRUCTIONS");

        ref readonly var node = ref TAST.NodeAt(RuleInst!.NodeId);
        var emitNodeId = TASI.AddNode(
            emitId.ParentId, emitId.Index, _instructCount, count - _instructCount,
            new(node.Args.OutCode, node.Args.RealmCode), RuleInst.RuleId
        );
        _instructCount = count;

        //EMIT NODES
        var caller = RuleInst;
        foreach (var emitNode in emitNodes)
            Emit(emitNode.NodeId, new(emitNodeId, emitNode.EmitId));

        RuleInst = caller;
    }
}

public readonly struct EmitNode(int emitId, int nodeId)
{
    public readonly int EmitId = emitId;
    public readonly int NodeId = nodeId;
}