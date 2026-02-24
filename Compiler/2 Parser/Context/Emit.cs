using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface EmitContext
{
    public void Emit(EmitId emitId = new(), params EmitNode[] emitNodes);

    //INSTRUCTIONS
    public const int BYTE_SIZE = TASI.BYTE_SIZE;
    public const int INT_SIZE = TASI.INT_SIZE;
    public const int REF_SIZE = TASI.REF_SIZE;

    public int WriteByte(byte value);
    public int WriteInt(int value);
    public int WriteObject(object value);
    public int WriteString(string value);

    public void AddInstruction(Lowerer.RuleId ruleId, int start, int length, Slice source = default);
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
            emitId.ParentId, emitId.Index,
            _instructCount, count - _instructCount,
            RuleInst.RuleId
        );
        _instructCount = count;

        //EMIT NODES
        var caller = RuleInst;
        foreach (var emitNode in emitNodes)
            Emit(emitNode.NodeId, new(emitNodeId, emitNode.EmitId));

        RuleInst = caller;
    }

    //INSTRUCTIONS
    public int WriteByte(byte value) => TASI.WriteByte(value);
    public int WriteInt(int value) => TASI.WriteInt(value);
    public int WriteObject(object value) => TASI.WriteObject(value);
    public int WriteString(string value) => TASI.WriteString(value);

    public void AddInstruction(Lowerer.RuleId ruleId, int start, int length, Slice source = default)
    => TASI.NewInstruction(ruleId, start, length, source);
}

public readonly struct EmitNode(int emitId, int nodeId)
{
    public readonly int EmitId = emitId;
    public readonly int NodeId = nodeId;
}