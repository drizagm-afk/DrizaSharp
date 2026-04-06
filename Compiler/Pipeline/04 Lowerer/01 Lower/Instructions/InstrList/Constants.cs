using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Constant
{
    //>>>> INT 32 <<<<
    public static InstrType Int32(this EmitContext ctx, int value)
    {
        (ctx as EmitInstrContext)!.WriteInt32(value);
        return InstrType.LdcInt32;
    }
    internal static void Rule_Int32(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldc_I4, ctx.ReadInt32()));
    }

    //>>>> INT 64 <<<<
    public static InstrType Int64(this EmitContext ctx, long value)
    {
        (ctx as EmitInstrContext)!.WriteInt64(value);
        return InstrType.LdcInt64;
    }
    internal static void Rule_Int64(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldc_I8, ctx.ReadInt64()));
    }

    //>>>> FLOAT 32 <<<<
    public static InstrType Float32(this EmitContext ctx, float value)
    {
        (ctx as EmitInstrContext)!.WriteFloat32(value);
        return InstrType.LdcFloat32;
    }
    internal static void Rule_Float32(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldc_R4, ctx.ReadFloat32()));
    }

    //>>>> FLOAT 64 <<<<
    public static InstrType Float64(this EmitContext ctx, double value)
    {
        (ctx as EmitInstrContext)!.WriteFloat64(value);
        return InstrType.LdcFloat64;
    }
    internal static void Rule_Float64(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldc_R8, ctx.ReadFloat64()));
    }

    //>>>> STRING <<<<
    public static InstrType String(this EmitContext ctx, string value)
    {
        (ctx as EmitInstrContext)!.WriteString(value);
        return InstrType.Ldstr;
    }
    internal static void Rule_String(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldstr, ctx.ReadString()));
    }
}