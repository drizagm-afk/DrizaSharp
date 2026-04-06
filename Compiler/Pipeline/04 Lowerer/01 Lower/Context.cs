using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public interface Context
{
    public AssemblyDefinition Assembly { get; }
    public AssemblyNameDefinition AssemblyName { get; }
    public ModuleDefinition Module => Assembly.MainModule;
}
public partial class LowererProcess : Context
{
    public AssemblyDefinition Assembly => _asm;
    public AssemblyNameDefinition AssemblyName => _asmName;
}