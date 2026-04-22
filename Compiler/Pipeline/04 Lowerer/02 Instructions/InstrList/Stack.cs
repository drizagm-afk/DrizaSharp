using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> STACK <<<<
public static partial class InstrContext
{
    public static InstrType Dup(this EmitContext _)
    => InstrType.Dup;
    public static InstrType Pop(this EmitContext _)
    => InstrType.Pop;
}
public partial class LowererProcess
{
    private void InstrDup()
    {
        const string name = "DUP";
        PopOnce(name, out var mono);
        Push(mono);
        Push(mono);

        _il.Append(_il.Create(OpCodes.Dup));
    }
    private void InstrPop()
    {
        const string name = "POP";
        PopOnce(name, out _);

        _il.Append(_il.Create(OpCodes.Pop));
    }
}