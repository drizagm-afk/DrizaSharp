using Mono.Cecil.Cil;

using DrzSharp.Compiler.Lowerer;
using DrzSharp.Compiler.Model;
using Instr = DrzSharp.Compiler.Model.Instruction;
using EmitCtx = DrzSharp.Compiler.Parser.EmitContext;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Logic
{
    public static class Load
    {
        public static class CStr
        {
            public static RuleId Id { get; internal set; }
            public static void Rule(Context ctx, Instr i)
            {
                var il = ctx.Logic.ILProcessor;
                il.Append(il.Create(OpCodes.Ldstr, ctx.ReadString(i.Start)));
            }

            private static Slice Add(EmitCtx ctx, string content)
            {
                var srt = ctx.WriteString(content);
                return new(srt, TASI.REF_SIZE);
            }
            public static void New(EmitCtx ctx, int source, string content)
            => ctx.AddInstruction(Id, Add(ctx, content), source);
            public static void New(EmitCtx ctx, Slice source, string content)
            => ctx.AddInstruction(Id, Add(ctx, content), source);
        }
    }

    public static class Print
    {
        public static RuleId Id { get; internal set; }
        public static void Rule(Context ctx, Instr _)
        {
            var il = ctx.Logic.ILProcessor;
            var writeLineRef = ctx.Module.ImportReference(
                typeof(Console).GetMethod("WriteLine", [typeof(string)])
            );
            il.Append(il.Create(OpCodes.Call, writeLineRef));
        }

        private static Slice Add(EmitCtx ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitCtx ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitCtx ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }

    public static class Return
    {
        public static RuleId Id { get; internal set; }
        public static void Rule(Context ctx, Instr _)
        {
            var il = ctx.Logic.ILProcessor;
            il.Append(il.Create(OpCodes.Ret));
        }

        private static Slice Add(EmitCtx ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitCtx ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitCtx ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
}