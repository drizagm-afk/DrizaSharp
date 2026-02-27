using Mono.Cecil;

using DrzSharp.Compiler.Lowerer;
using DrzSharp.Compiler.Model;
using Instr = DrzSharp.Compiler.Model.Instruction;
using EmitCtx = DrzSharp.Compiler.Parser.EmitContext;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Virtual
{
    public static class EntryPoint
    {
        public static int Id { get; internal set; }
        public static void Rule(Context ctx, Instr _)
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

        private static Slice Add(EmitCtx ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitCtx ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitCtx ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
}