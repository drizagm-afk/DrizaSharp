using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler.Lowerer;

//>>>> STORAGE LOCAL <<<<
public static partial class InstrContext
{
    public static InstrType LoadLocal(this EmitContext ctx, int localId)
    {
        ((EmitInstrContext)ctx).WriteInt32(localId);
        return InstrType.LoadLocal;
    }
    public static InstrType LoadLocalAddress(this EmitContext ctx, int localId)
    {
        ((EmitInstrContext)ctx).WriteInt32(localId);
        return InstrType.LoadLocalAddress;
    }
    public static InstrType StoreLocal(this EmitContext ctx, int localId)
    {
        ((EmitInstrContext)ctx).WriteInt32(localId);
        return InstrType.StoreLocal;
    }
    public static InstrType DeclLocal(this EmitContext ctx, int localId)
    {
        ((EmitInstrContext)ctx).WriteInt32(localId);
        return InstrType.DeclLocal;
    }
}
public partial class LowererProcess
{
    private void InstrLoadLocal()
    {
        var localId = ReadInt32();
        if (_locals.Count <= localId)
            throw new AbortException($"Tried to Load from LOCAL_{localId} before declaring it");

        var localDef = _locals[localId];
        _il.Append(_il.Create(OpCodes.Ldloc, localDef));
    }
    private void InstrLoadLocalAddress()
    {
        var localId = ReadInt32();
        if (_locals.Count <= localId)
            throw new AbortException($"Tried to Load Address from LOCAL_{localId} before declaring it");

        var localDef = _locals[localId];
        _il.Append(_il.Create(OpCodes.Ldloca, localDef));
    }
    private void InstrStoreLocal()
    {
        var localId = ReadInt32();
        if (_locals.Count <= localId)
            throw new AbortException($"Tried to Store in LOCAL_{localId} before declaring it");

        var localDef = _locals[localId];
        _il.Append(_il.Create(OpCodes.Stloc, localDef));
    }
    private void InstrDeclLocal()
    {
        var localId = ReadInt32();
        if (_locals.Count != localId)
            throw new AbortException($"Tried to Declare LOCAL_{localId} after higher LocalIds");

        _locals.Add(new(TYPE_INT32));
    }
}

//>>>> STORAGE ARGS <<<<
public static partial class InstrContext
{
    public static InstrType LoadArg(this EmitContext ctx, int argId)
    {
        ((EmitInstrContext)ctx).WriteInt32(argId);
        return InstrType.LoadLocal;
    }
    public static InstrType LoadArgAddress(this EmitContext ctx, int argId)
    {
        ((EmitInstrContext)ctx).WriteInt32(argId);
        return InstrType.LoadLocalAddress;
    }
    public static InstrType StoreArg(this EmitContext ctx, int argId)
    {
        ((EmitInstrContext)ctx).WriteInt32(argId);
        return InstrType.StoreLocal;
    }
}
public partial class LowererProcess
{
}

//>>>> STORAGE FIELD <<<<
public static partial class InstrContext
{
    public static InstrType LoadField(this EmitContext ctx, UDeclType utype, GlobalId fieldId)
    => LoadField(ctx, UContext.GetDeclMember(utype, fieldId));
    public static InstrType LoadField(this EmitContext ctx, UDeclMember ufield)
    {
        ((EmitInstrContext)ctx).WriteObject(ufield);
        return InstrType.LoadField;
    }

    public static InstrType LoadFieldAddress(this EmitContext ctx, UDeclType utype, GlobalId fieldId)
    => LoadFieldAddress(ctx, UContext.GetDeclMember(utype, fieldId));
    public static InstrType LoadFieldAddress(this EmitContext ctx, UDeclMember ufield)
    {
        ((EmitInstrContext)ctx).WriteObject(ufield);
        return InstrType.LoadFieldAddress;
    }

    public static InstrType StoreField(this EmitContext ctx, UDeclType utype, GlobalId fieldId)
    => StoreField(ctx, UContext.GetDeclMember(utype, fieldId));
    public static InstrType StoreField(this EmitContext ctx, UDeclMember ufield)
    {
        ((EmitInstrContext)ctx).WriteObject(ufield);
        return InstrType.StoreField;
    }
}
public partial class LowererProcess
{
}