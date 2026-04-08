using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Flow
{
    //>>>> RETURN <<<<
    public static InstrType Return(this EmitContext _)
    => InstrType.Return;
    internal static void Rule_Return(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ret));
    }
}