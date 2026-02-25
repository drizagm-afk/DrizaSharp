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
    public const int BYTE_SIZE = TASI.BYTE_SIZE;
    public const int INT_SIZE = TASI.INT_SIZE;
    public const int REF_SIZE = TASI.REF_SIZE;

    public byte ReadByte(int offset);
    public int ReadInt(int offset);
    public T ReadObject<T>(int offset);
    public string ReadString(int offset);
}

public partial class LowererProcess : Context
{
    public LogicContext Logic => this;
    public VirtualContext Virtual => this;

    public AssemblyDefinition Assembly => _asm;
    public ModuleDefinition Module { get; private set; } = null!;

    public byte ReadByte(int offset) => TASI.ReadByte(offset);
    public int ReadInt(int offset) => TASI.ReadInt(offset);
    public T ReadObject<T>(int offset) => (T)TASI.ReadObject(offset);
    public string ReadString(int offset) => TASI.ReadString(offset);
}