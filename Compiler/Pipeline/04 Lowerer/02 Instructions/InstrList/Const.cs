using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> CONST <<<<
public static partial class InstrContext
{
    public static InstrType LoadInt32(this EmitContext ctx, int value)
    {
        ((EmitInstrContext)ctx).WriteInt32(value);
        return InstrType.LoadInt32;
    }
    public static InstrType LoadInt64(this EmitContext ctx, long value)
    {
        ((EmitInstrContext)ctx).WriteInt64(value);
        return InstrType.LoadInt64;
    }
    public static InstrType LoadFloat32(this EmitContext ctx, float value)
    {
        ((EmitInstrContext)ctx).WriteFloat32(value);
        return InstrType.LoadFloat32;
    }
    public static InstrType LoadFloat64(this EmitContext ctx, double value)
    {
        ((EmitInstrContext)ctx).WriteFloat64(value);
        return InstrType.LoadFloat64;
    }
    public static InstrType LoadString(this EmitContext ctx, string value)
    {
        ((EmitInstrContext)ctx).WriteString(value);
        return InstrType.LoadString;
    }
    public static InstrType Null(this EmitContext _)
    => InstrType.LoadNull;
}
public partial class LowererProcess
{
    private void InstrLoadInt32()
    {
        Push(ToUsage(CTX.TYPE_INT32));

        _il.Append(_il.Create(OpCodes.Ldc_I4, ReadInt32()));
    }
    private void InstrLoadInt64()
    {
        Push(ToUsage(CTX.TYPE_INT64));

        _il.Append(_il.Create(OpCodes.Ldc_I8, ReadInt64()));
    }
    private void InstrLoadFloat32()
    {
        Push(ToUsage(CTX.TYPE_FLOAT32));

        _il.Append(_il.Create(OpCodes.Ldc_R4, ReadFloat32()));
    }
    private void InstrLoadFloat64()
    {
        Push(ToUsage(CTX.TYPE_FLOAT64));

        _il.Append(_il.Create(OpCodes.Ldc_R8, ReadFloat64()));
    }
    private void InstrLoadString()
    {
        Push(ToUsage(CTX.TYPE_STRING));

        _il.Append(_il.Create(OpCodes.Ldstr, ReadString()));
    }
    private void InstrLoadNull()
    {
        Push(UContext.Null);

        _il.Append(_il.Create(OpCodes.Ldnull));
    }
}