using Mono.Cecil;
using Mono.Cecil.Cil;

using EmitContext = DrzSharp.Compiler.Parser.EmitContext;
using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Virtual
{
    public static int EntryPoint_Id { get; internal set; }
    public static int EntryPoint(EmitContext _) => EntryPoint_Id;

    public static int InitASMMethod_Id { get; internal set; }
    public static int InitASMMethod(EmitContext ctx, int labelCount)
    {
        ctx.WriteInt(labelCount);
        return InitASMMethod_Id;
    }
}

public static partial class VirtualRules
{
    public static void EntryPoint(Context ctx)
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
        ctx.Virtual.EnterMethod(body);
    }

    public static void InitASMMethod(Context ctx)
    {
        var labelCount = ctx.ReadInt();

        var il = ctx.Logic.IL;
        for (int j = 0; j < labelCount; j++)
            ctx.Logic.Labels.Add(il.Create(OpCodes.Nop));
    }
}