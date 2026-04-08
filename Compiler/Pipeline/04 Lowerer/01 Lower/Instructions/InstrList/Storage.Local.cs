using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Local
{
    //>>>> DECL LOCAL <<<<
    public static InstrType Declare(this EmitContext ctx, int varId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(varId);
        return InstrType.DeclLocal;
    }
    internal static void Rule_Declare(InstrContext ctx)
    {
        var vars = ctx.Variables;

        var val = ctx.ReadInt32();
        Console.WriteLine($"{vars.Count} : {val}");
        if (vars.Count != val)
            throw new Exception();

        vars.Add(new(ctx.Module.TypeSystem.Int32));
    }

    //>>>> LOAD LOCAL <<<<
    public static InstrType Load(this EmitContext ctx, int varId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(varId);
        return InstrType.LoadLocal;
    }
    internal static void Rule_Load(InstrContext ctx)
    {
        var localDef = ctx.Variables[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ldloc, localDef));
    }

    //>>>> STORE LOCAL <<<<
    public static InstrType Store(this EmitContext ctx, int varId)
    {
        (ctx as EmitInstrContext)!.WriteInt32(varId);
        return InstrType.StoreLocal;
    }
    internal static void Rule_Store(InstrContext ctx)
    {
        var localDef = ctx.Variables[ctx.ReadInt32()];

        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Stloc, localDef));
    }
}