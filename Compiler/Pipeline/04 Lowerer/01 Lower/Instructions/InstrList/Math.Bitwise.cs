using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Bitwise
{
    //>>>> AND <<<<
    public static InstrType And(this EmitContext _)
    => InstrType.And;
    internal static void Rule_And(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.And));
    }

    //>>>> OR <<<<
    public static InstrType Or(this EmitContext _)
    => InstrType.Or;
    internal static void Rule_Or(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Or));
    }

    //>>>> XOR <<<<
    public static InstrType Xor(this EmitContext _)
    => InstrType.Xor;
    internal static void Rule_Xor(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Xor));
    }

    //>>>> NOT <<<<
    public static InstrType Not(this EmitContext _)
    => InstrType.Not;
    internal static void Rule_Not(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Not));
    }

    //>>>> SHIFT LEFT <<<<
    public static InstrType ShiftLeft(this EmitContext _)
    => InstrType.ShiftLeft;
    internal static void Rule_ShiftLeft(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Shl));
    }

    //>>>> SHIFT RIGHT <<<<
    public static InstrType ShiftRight(this EmitContext _)
    => InstrType.ShiftRight;
    internal static void Rule_ShiftRight(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Shr));
    }
}