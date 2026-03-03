using Mono.Cecil;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lowerer;

public interface Context
{
    //CONTEXTS
    public LogicContext Logic { get; }
    public VirtualContext Virtual { get; }

    //ASSEMBLY
    public AssemblyDefinition Assembly { get; }
    public ModuleDefinition Module { get; }

    //INSTRUCTIONS
    public Instruction Instruction { get; }
    public byte ReadByte();
    public int ReadInt();
    public T ReadObject<T>();
    public string ReadString();
}

public partial class LowererProcess : Context
{
    public LogicContext Logic => this;
    public VirtualContext Virtual => this;

    public AssemblyDefinition Assembly => _asm;
    public ModuleDefinition Module { get; private set; } = null!;

    public Instruction Instruction => _instr;
    private Instruction _instr;

    private int _offset;
    public byte ReadByte()
    {
        var val = TASI.ReadByte(_offset);
        _offset += TASI.BYTE_SIZE;
        return val;
    }
    public int ReadInt()
    {
        var val = TASI.ReadInt(_offset);
        _offset += TASI.INT_SIZE;
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