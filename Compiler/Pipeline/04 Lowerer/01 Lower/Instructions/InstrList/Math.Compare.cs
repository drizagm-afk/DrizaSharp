using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Compare
{
    //>>>> EQUAL <<<<
    public static InstrType Equal(this EmitContext _)
    => InstrType.Equal;
    internal static void Rule_Equal(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ceq));
    }

    //>>>> GREATER THAN <<<<
    public static InstrType GreaterThan(this EmitContext _)
    => InstrType.GreaterThan;
    internal static void Rule_GreaterThan(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Cgt));
    }

    //>>>> LESS THAN <<<<
    public static InstrType LessThan(this EmitContext _)
    => InstrType.LessThan;
    internal static void Rule_LessThan(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Clt));
    }
}