using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Logic
{
    public static class Ldstr
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, string content)
        {
            var srt = ctx.WriteString(content);
            return new(srt, TASI.REF_SIZE);
        }
        public static void New(EmitContext ctx, int source, string content)
        => ctx.AddInstruction(Id, Add(ctx, content), source);
        public static void New(EmitContext ctx, Slice source, string content)
        => ctx.AddInstruction(Id, Add(ctx, content), source);
    }

    public static class Print
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }

    public static class Ret
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
}