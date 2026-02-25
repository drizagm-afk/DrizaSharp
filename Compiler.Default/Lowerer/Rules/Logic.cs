using Mono.Cecil.Cil;

using DrzSharp.Compiler.Lowerer;
using Instr = DrzSharp.Compiler.Model.Instruction;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Rules
{
    public static void Ldstr(Context ctx, Instr i)
    {
        var il = ctx.Logic.ILProcessor;
        il.Append(il.Create(OpCodes.Ldstr, ctx.ReadString(i.Start)));
    }

    public static void Print(Context ctx, Instr _)
    {
        var il = ctx.Logic.ILProcessor;
        var writeLineRef = ctx.Module.ImportReference(
            typeof(Console).GetMethod("WriteLine", [typeof(string)])
        );
        il.Append(il.Create(OpCodes.Call, writeLineRef));
    }
    public static void Ret(Context ctx, Instr _)
    {
        var il = ctx.Logic.ILProcessor;
        il.Append(il.Create(OpCodes.Ret));
    }
}