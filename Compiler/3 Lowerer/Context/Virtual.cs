using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public interface VirtualContext
{
    public void EnterLogic(ILProcessor iLProcessor);
}
public partial class LowererProcess : VirtualContext
{
    public void EnterLogic(ILProcessor iLProcessor)
    {
        ILProcessor = iLProcessor;
        ResetLogic();
    }

    //RESET
    internal void ResetVirtual()
    {
        Module = null!;
    }
}