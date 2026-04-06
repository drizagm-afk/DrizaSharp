using System.Collections.Immutable;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

//>>>> EMIT <<<<
public interface EmitContext : Context
{
    public void AddInstr(InstrType type);
    public void AddInstr(InstrType type, int sourceNode);
    public void AddInstr(InstrType type, SourceSlice source);
    public void AddInnerEmit(int nodeId);

    public void Emit(EmitTarget? emitTarget = null);
}
public partial class ParserProcess : EmitContext
{
    private int _instrCount;
    public void AddInstr(InstrType type)
    => AddInstr(type, RuleInst!.NodeId);
    public void AddInstr(InstrType type, int sourceNode)
    => AddInstr(type, TAST.SourceSlice(sourceNode));
    public void AddInstr(InstrType type, SourceSlice source)
    {
        TASI.NewInstruction(type, _dataCount, TASI.DataCount - _dataCount, source);
        _dataCount = TASI.DataCount;
    }

    private ImmutableArray<(int, int)>.Builder _innerEmits = ImmutableArray.CreateBuilder<(int, int)>();
    public void AddInnerEmit(int nodeId)
    => _innerEmits.Add((nodeId, TASI.DataCount - _dataCount));

    public void Emit(EmitTarget? nullTarget)
    {
        if (nullTarget is not EmitTarget target)
            target = RuleInst!.EmitTarget;

        //EMIT
        var count = TASI.InstructionCount;
        if (_instrCount == count)
            throw new Exception("Cannot Emit with zero Instructions");

        var emitNode = TASI.AddNode(
            target.NodeId, target.InstrId,
            _instrCount, count - _instrCount,
            new(RuleInst!.NodeId)
        );

        var inner = _innerEmits.ToImmutable();
        _instrCount = count;

        //EMIT NODES
        var caller = RuleInst;
        foreach (var (nodeId, instrId) in inner)
            Emit(TAST.NodeAt(nodeId), new(emitNode, instrId));

        RuleInst = caller;
    }
}

//>>>> EMIT INSTR <<<<
internal interface EmitInstrContext
{
    internal int WriteByte(byte value);
    internal int WriteInt32(int value);
    internal int WriteInt64(long value);
    internal int WriteFloat32(float value);
    internal int WriteFloat64(double value);

    internal int WriteObject(object value);
    internal int WriteString(string value);
}
public partial class ParserProcess : EmitInstrContext
{
    private int _dataCount = 0;
    public int WriteByte(byte value) => TASI.WriteByte(value);
    public int WriteInt32(int value) => TASI.WriteInt32(value);
    public int WriteInt64(long value) => TASI.WriteInt64(value);
    public int WriteFloat32(float value) => TASI.WriteFloat32(value);
    public int WriteFloat64(double value) => TASI.WriteFloat64(value);

    public int WriteObject(object value) => TASI.WriteObject(value);
    public int WriteString(string value) => TASI.WriteString(value);
}