using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Stack
{
    //>>>> DUPLICATE <<<<
    public static InstrType Dup(this EmitContext _)
    => InstrType.Dup;
    internal static void Rule_Dup(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Dup));
    }

    //>>>> POP <<<<
    public static InstrType Pop(this EmitContext _)
    => InstrType.Pop;
    internal static void Rule_Pop(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Pop));
    }
}