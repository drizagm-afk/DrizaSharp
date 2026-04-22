using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> TYPE STRUCT <<<<
public static partial class InstrContext
{
    public static InstrType Unbox(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.Unbox;
    }
    public static InstrType UnboxAddress(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.UnboxAddress;
    }
    public static InstrType Box(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.Box;
    }
}
public partial class LowererProcess
{
}

//>>>> TYPE ARRAY <<<<
public static partial class InstrContext
{
    public static InstrType NewArray(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.NewArray;
    }
    public static InstrType LoadLength(this EmitContext _)
    => InstrType.LoadLength;
    public static InstrType LoadElement(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.LoadElement;
    }
    public static InstrType LoadElementAddress(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.LoadElementAddress;
    }
    public static InstrType StoreElement(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.StoreElement;
    }
}
public partial class LowererProcess
{
}

//>>>> TYPE CAST <<<<
public static partial class InstrContext
{
    public static InstrType CastTo(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.CastTo;
    }
    public static InstrType TryCastTo(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.TryCastTo;
    }
}
public partial class LowererProcess
{
}

//>>>> TYPE ADDRESS <<<<
public static partial class InstrContext
{
    public static InstrType LoadFromAddress(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.LoadFromAddress;
    }
    public static InstrType StoreAtAddress(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.StoreAtAddress;
    }
    public static InstrType InitAtAddress(this EmitContext ctx, UType utype)
    {
        ((EmitInstrContext)ctx).WriteObject(utype);
        return InstrType.InitAtAddress;
    }
}
public partial class LowererProcess
{
}