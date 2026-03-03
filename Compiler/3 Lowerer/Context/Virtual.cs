using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public interface VirtualContext
{
    public void EnterMethod(MethodBody body);
}
public partial class LowererProcess : VirtualContext
{
    public void EnterMethod(MethodBody body)
    {
        ResetLogic();
        MethodBody = body;
    }

    //RESET
    internal void ResetVirtual()
    {
        Module = null!;
    }
}