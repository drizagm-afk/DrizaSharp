using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public static class Special
{
    //>>>> ENTER METHOD <<<<
    public static InstrType EnterMethod(this EmitContext _, int labelCount, int localCount)
    {
        var ctx = (EmitInstrContext)_;
        ctx.WriteInt32(labelCount);
        ctx.WriteInt32(localCount);
        return InstrType.EnterMethod;
    }
    internal static void Rule_EnterMethod(InstrContext ctx)
    {
        //ADD PROGRAM TYPE
        var programType = new TypeDefinition(
            "", "Program",
            TypeAttributes.Public | TypeAttributes.Class,
            ctx.Module.TypeSystem.Object
        );
        ctx.Module.Types.Add(programType);

        //ADD MAIN METHOD
        var mainMethod = new MethodDefinition(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            ctx.Module.TypeSystem.Void
        );

        programType.Methods.Add(mainMethod);
        ctx.Assembly.EntryPoint = mainMethod;

        var body = mainMethod.Body;
        body.InitLocals = true;
        ctx.EnterMethod(body);

        //INIT METHOD
        var labelCount = ctx.ReadInt32();
        var localCount = ctx.ReadInt32();

        var il = ctx.IL;
        for (int j = 0; j < labelCount; j++)
            ctx.Labels.Add(il.Create(OpCodes.Nop));
    }

    //>>>> CALL PRINT <<<<
    public static InstrType Print(this EmitContext _)
    => InstrType.Print;
    internal static void Rule_Print(InstrContext ctx)
    {
        var il = ctx.IL;
        var writeLineRef = ctx.Module.ImportReference(
            typeof(Console).GetMethod("WriteLine", [typeof(int)])
        );
        il.Append(il.Create(OpCodes.Call, writeLineRef));
    }

    //>>>> RETURN <<<<
    public static InstrType Return(this EmitContext _)
    => InstrType.Return;
    internal static void Rule_Return(InstrContext ctx)
    {
        var il = ctx.IL;
        il.Append(il.Create(OpCodes.Ret));
    }
}