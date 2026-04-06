using DrzSharp.Compiler.Model;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler.Lowerer;

public interface InstrContext : Context
{
    public MethodBody MethodBody { get; }
    public ILProcessor IL { get; }
    public Collection<VariableDefinition> Variables { get; }
    public List<Instruction> Labels { get; }

    public void EnterMethod(MethodBody body);

    //INSTRUCTIONS
    public Instr Instruction { get; }
    public byte ReadByte();
    public int ReadInt32();
    public long ReadInt64();
    public float ReadFloat32();
    public double ReadFloat64();

    public T ReadObject<T>();
    public string ReadString();
}
public partial class LowererProcess : InstrContext
{
    public MethodBody MethodBody { get; private set; } = null!;
    public ILProcessor IL => MethodBody.GetILProcessor();
    public Collection<VariableDefinition> Variables => MethodBody.Variables;
    public List<Instruction> Labels { get; private set; } = [];

    public void EnterMethod(MethodBody body)
    {
        MethodBody = body;
        Labels.Clear();
    }

    //INSTRUCTIONS
    public Instr Instruction { get; private set; }

    private int _offset;
    public byte ReadByte()
    {
        var val = TASI.ReadByte(_offset);
        _offset += TASI.BYTE_SIZE;
        return val;
    }
    public int ReadInt32()
    {
        var val = TASI.ReadInt32(_offset);
        _offset += TASI.INT32_SIZE;
        return val;
    }
    public long ReadInt64()
    {
        var val = TASI.ReadInt64(_offset);
        _offset += TASI.INT64_SIZE;
        return val;
    }
    public float ReadFloat32()
    {
        var val = TASI.ReadFloat32(_offset);
        _offset += TASI.FLOAT32_SIZE;
        return val;
    }
    public double ReadFloat64()
    {
        var val = TASI.ReadFloat64(_offset);
        _offset += TASI.FLOAT64_SIZE;
        return val;
    }

    public T ReadObject<T>()
    {
        var val = TASI.ReadObject(_offset);
        _offset += TASI.REF_SIZE;
        return (T)val;
    }
    public string ReadString()
    {
        var val = TASI.ReadString(_offset);
        _offset += TASI.REF_SIZE;
        return val;
    }
}