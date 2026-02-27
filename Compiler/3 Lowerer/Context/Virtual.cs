using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public interface VirtualContext
{
    public void SetILProcessor(ILProcessor iLProcessor);
}
public partial class LowererProcess : VirtualContext
{
    public void SetILProcessor(ILProcessor iLProcessor)
    {
        ResetLogic();
        ILProcessor = iLProcessor;
    }

    //RESET
    internal void ResetVirtual()
    {
        Module = null!;
    }
}