using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> CALL METHOD <<<<
public static partial class InstrContext
{
    public static InstrType Call(this EmitContext ctx, UDeclType utype, GlobalId methodId)
    => Call(ctx, UContext.GetDeclMember(utype, methodId));
    public static InstrType Call(this EmitContext ctx, UDeclType utype, GlobalId methodId, params ArrayView<UType> args)
    => Call(ctx, UContext.GetDeclMember(utype, methodId, args));
    public static InstrType Call(this EmitContext ctx, UDeclMember umethod)
    {
        ((EmitInstrContext)ctx).WriteObject(umethod);
        return InstrType.Call;
    }

    public static InstrType CallVirt(this EmitContext ctx, UDeclType utype, GlobalId method)
    => CallVirt(ctx, UContext.GetDeclMember(utype, method));
    public static InstrType CallVirt(this EmitContext ctx, UDeclType utype, GlobalId method, params ArrayView<UType> args)
    => CallVirt(ctx, UContext.GetDeclMember(utype, method, args));
    public static InstrType CallVirt(this EmitContext ctx, UDeclMember umethod)
    {
        ((EmitInstrContext)ctx).WriteObject(umethod);
        return InstrType.CallVirt;
    }

    public static InstrType NewObject(this EmitContext ctx, UDeclType utype, GlobalId method)
    => NewObject(ctx, UContext.GetDeclMember(utype, method, default));
    public static InstrType NewObject(this EmitContext ctx, UDeclMember uctor)
    {
        ((EmitInstrContext)ctx).WriteObject(uctor);
        return InstrType.NewObject;
    }
}
public partial class LowererProcess
{
    private void InstrCall()
    {
        
    }
}