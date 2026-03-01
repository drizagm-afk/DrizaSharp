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
}