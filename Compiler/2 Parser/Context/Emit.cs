using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface EmitContext : Context
{
    public void Emit(TASTEmit emitId = new(), params EmitNode[] emitNodes);

    //INSTRUCTIONS
    public int WriteByte(byte value);
    public int WriteInt(int value);
    public int WriteObject(object value);
    public int WriteString(string value);

    public void EmitInstr(int ruleId);
    public void EmitInstr(int ruleId, int source);
    public void EmitInstr(int ruleId, Slice source);
}

public partial class ParserProcess : EmitContext
{
    private int _instructCount = 0;
    public void Emit(TASTEmit emitId = new(), params EmitNode[] emitNodes)
    {
        if (!emitId.IsValid) emitId = RuleInst!.EmitId;

        //EMIT
        var count = TASI.InstructionCount;
        if (_instructCount == count)
            throw new Exception("CANNOT EMIT WITH ZERO INSTRUCTIONS");

        ref readonly var node = ref TAST.NodeAt(RuleInst!.NodeId);
        var emitNodeId = TASI.AddNode(
            emitId.ParentId, emitId.Index,
            _instructCount, count - _instructCount,
            new(RuleInst.NodeId)
        );
        _instructCount = count;

        //EMIT NODES
        var caller = RuleInst;
        foreach (var emitNode in emitNodes)
            Emit(emitNode.NodeId, new(emitNodeId, emitNode.EmitId));

        RuleInst = caller;
    }

    //INSTRUCTIONS
    private int _dataCount = 0;
    public int WriteByte(byte value) => TASI.WriteByte(value);
    public int WriteInt(int value) => TASI.WriteInt(value);
    public int WriteObject(object value) => TASI.WriteObject(value);
    public int WriteString(string value) => TASI.WriteString(value);

    public void EmitInstr(int ruleId)
    => EmitInstr(ruleId, RuleInst!.NodeId);
    public void EmitInstr(int ruleId, int source)
    => EmitInstr(ruleId, TAST.SourceSlice(source));
    public void EmitInstr(int ruleId, Slice source)
    {
        TASI.NewInstruction(ruleId, _dataCount, _dataCount - TASI.DataCount, source);
        _dataCount = TASI.DataCount;
    }
}

public readonly struct EmitNode(int emitId, int nodeId)
{
    public readonly int EmitId = emitId;
    public readonly int NodeId = nodeId;
}