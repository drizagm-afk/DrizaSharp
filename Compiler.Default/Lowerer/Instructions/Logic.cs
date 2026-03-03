using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Logic
{
    //LOCALS
    public static class NewLoc
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, 0);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
    public static class LdLoc
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int varId)
        {
            var srt = ctx.WriteInt(varId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int varId)
        => ctx.AddInstruction(Id, Add(ctx, varId), source);
        public static void New(EmitContext ctx, Slice source, int varId)
        => ctx.AddInstruction(Id, Add(ctx, varId), source);
    }
    public static class StLoc
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int varId)
        {
            var srt = ctx.WriteInt(varId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int varId)
        => ctx.AddInstruction(Id, Add(ctx, varId), source);
        public static void New(EmitContext ctx, Slice source, int varId)
        => ctx.AddInstruction(Id, Add(ctx, varId), source);
    }

    //BRANCHES
    public static class NewBr
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int branchId)
        {
            var srt = ctx.WriteInt(branchId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
        public static void New(EmitContext ctx, Slice source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
    }
    public static class Br
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int branchId)
        {
            var srt = ctx.WriteInt(branchId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
        public static void New(EmitContext ctx, Slice source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
    }
    public static class BrTrue
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int branchId)
        {
            var srt = ctx.WriteInt(branchId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
        public static void New(EmitContext ctx, Slice source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
    }
    public static class BrFalse
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int branchId)
        {
            var srt = ctx.WriteInt(branchId);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
        public static void New(EmitContext ctx, Slice source, int branchId)
        => ctx.AddInstruction(Id, Add(ctx, branchId), source);
    }

    //COMPARISONS
    public static class Ceq
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
    public static class Cgt
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
    public static class Clt
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }

    //ARITHMETIC
    public static class Add
    {
        public static int Id { get; internal set; }
        private static Slice AddI(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, AddI(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, AddI(ctx), source);
    }
    public static class Sub
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
    public static class Mul
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }
    public static class Div
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx)
        => new(ctx.DataCount, TASI.INT_SIZE);
        public static void New(EmitContext ctx, int source)
        => ctx.AddInstruction(Id, Add(ctx), source);
        public static void New(EmitContext ctx, Slice source)
        => ctx.AddInstruction(Id, Add(ctx), source);
    }

    //CONSTANTS
    public static class LdcI4
    {
        public static int Id { get; internal set; }
        private static Slice Add(EmitContext ctx, int value)
        {
            var srt = ctx.WriteInt(value);
            return new(srt, TASI.INT_SIZE);
        }
        public static void New(EmitContext ctx, int source, int value)
        => ctx.AddInstruction(Id, Add(ctx, value), source);
        public static void New(EmitContext ctx, Slice source, int value)
        => ctx.AddInstruction(Id, Add(ctx, value), source);
    }
    public static class LdStr
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

    //METHODS
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