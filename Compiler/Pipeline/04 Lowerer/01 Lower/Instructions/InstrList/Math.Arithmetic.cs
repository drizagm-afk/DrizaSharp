using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Arith
{
    //>>>> ADD <<<<
    public static InstrType Add(this EmitContext _)
    => InstrType.Add;
    internal static void Rule_Add(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Add));
    }

    //>>>> SUBTRACT <<<<
    public static InstrType Sub(this EmitContext _)
    => InstrType.Sub;
    internal static void Rule_Sub(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Sub));
    }

    //>>>> MULTIPLY <<<<
    public static InstrType Mul(this EmitContext _)
    => InstrType.Mul;
    internal static void Rule_Mul(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Mul));
    }

    //>>>> DIVIDE <<<<
    public static InstrType Div(this EmitContext _)
    => InstrType.Div;
    internal static void Rule_Div(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Div));
    }

    //>>>> REMANENT <<<<
    public static InstrType Rem(this EmitContext _)
    => InstrType.Rem;
    internal static void Rule_Rem(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Rem));
    }
}