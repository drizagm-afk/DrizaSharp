using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Virtual
{
    public static class EntryPoint
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }

    public static class InitASMMethod
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int labelCount)
        {
            var start = ctx.WriteInt(labelCount);
            return new(start, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int labelCount)
        => ctx.AddInstruction(Id, Add(ctx, labelCount), source);
        public static void New(EmitContext ctx, Slice source, int labelCount)
        => ctx.AddInstruction(Id, Add(ctx, labelCount), source);
    }
}