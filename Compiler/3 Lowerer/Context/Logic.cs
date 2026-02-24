using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public interface LogicContext
{
    public ILProcessor ILProcessor { get; }
}
public partial class LowererProcess : LogicContext
{
    public ILProcessor ILProcessor { get; private set; } = null!;

    internal void ResetLogic()
    {
        ILProcessor = null!;
    }
}