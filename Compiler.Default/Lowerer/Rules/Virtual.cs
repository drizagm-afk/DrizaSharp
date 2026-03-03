using Mono.Cecil;
using Mono.Cecil.Cil;

using DrzSharp.Compiler.Lowerer;
using Instr = DrzSharp.Compiler.Model.Instruction;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class VirtualRules
{
    public static void EntryPoint(Context ctx, Instr _)
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

    public static void InitASMMethod(Context ctx, Instr i)
    {
        var labelCount = ctx.ReadInt(i.Start);

        var il = ctx.Logic.IL;
        for (int j = 0; j < labelCount; j++)
            ctx.Logic.Labels.Add(il.Create(OpCodes.Nop));
    }
}