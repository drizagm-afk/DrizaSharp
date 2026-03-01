using Mono.Cecil;

using DrzSharp.Compiler.Lowerer;
using Instr = DrzSharp.Compiler.Model.Instruction;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Rules
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

        ctx.Virtual.SetILProcessor(mainMethod.Body.GetILProcessor());
    }
}