using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Branch
{
    //>>>> DECL LABEL <<<<
    public static InstrType Label(this EmitContext ctx, int labelId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(labelId);
        return InstrType.Label;
    }
    internal static void Rule_Label(InstrContext ctx)
    {
        var labelDef = ctx.Labels[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(labelDef);
    }

    //>>>> BRANCH <<<<
    public static InstrType Goto(this EmitContext ctx, int labelId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(labelId);
        return InstrType.Br;
    }
    internal static void Rule_Goto(InstrContext ctx)
    {
        var labelDef = ctx.Labels[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Br, labelDef));
    }

    //>>>> BRANCH IF TRUE <<<<
    public static InstrType GotoIfTrue(this EmitContext ctx, int labelId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(labelId);
        return InstrType.BrTrue;
    }
    internal static void Rule_GotoIfTrue(InstrContext ctx)
    {
        var labelDef = ctx.Labels[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Brtrue, labelDef));
    }

    //>>>> BRANCH IF FALSE <<<<
    public static InstrType GotoIfFalse(this EmitContext ctx, int labelId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(labelId);
        return InstrType.BrFalse;
    }
    internal static void Rule_GotoIfFalse(InstrContext ctx)
    {
        var labelDef = ctx.Labels[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Brfalse, labelDef));
    }
}