using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public interface VirtualContext
{
    public ModuleDefinition Module { get; }
}
public partial class LowererProcess : VirtualContext
{
    public ModuleDefinition Module { get; private set; } = null!;

    //RESET
    internal void ResetVirtual()
    {
        Module = null!;
    }
}