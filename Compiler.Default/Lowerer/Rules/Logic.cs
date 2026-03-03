using Mono.Cecil.Cil;

using DrzSharp.Compiler.Lowerer;
using Instr = DrzSharp.Compiler.Model.Instruction;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class LogicRules
{
    //LOCALS
    public static void NewLoc(Context ctx, Instr _)
    {
        ctx.Logic.Variables.Add(
            new(ctx.Module.TypeSystem.Int32)
        );
    }
    public static void LdLoc(Context ctx, Instr i)
    {
        var localDef = ctx.Logic.Variables[ctx.ReadInt(i.Start)];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldloc, localDef));
    }
    public static void StLoc(Context ctx, Instr i)
    {
        var localDef = ctx.Logic.Variables[ctx.ReadInt(i.Start)];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Stloc, localDef));
    }

    //BRANCHES
    public static void NewBr(Context ctx, Instr i)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt(i.Start)];
        var il = ctx.Logic.IL;

        il.Append(labelDef);
    }
    public static void Br(Context ctx, Instr i)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt(i.Start)];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Br, labelDef));
    }
    public static void BrTrue(Context ctx, Instr i)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt(i.Start)];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Brtrue, labelDef));
    }
    public static void BrFalse(Context ctx, Instr i)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt(i.Start)];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Brfalse, labelDef));
    }

    //COMPARISONS
    public static void Ceq(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ceq));
    }
    public static void Cgt(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Cgt));
    }
    public static void Clt(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Clt));
    }

    //ARITHMETIC
    public static void Add(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Add));
    }
    public static void Sub(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Sub));
    }
    public static void Mul(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Mul));
    }
    public static void Div(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Div));
    }

    //CONSTANTS
    public static void LdcI4(Context ctx, Instr i)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldc_I4, ctx.ReadInt(i.Start)));
    }
    public static void LdStr(Context ctx, Instr i)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldstr, ctx.ReadString(i.Start)));
    }

    //METHODS
    public static void Print(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        var writeLineRef = ctx.Module.ImportReference(
            typeof(Console).GetMethod("WriteLine", [typeof(int)])
        );
        il.Append(il.Create(OpCodes.Call, writeLineRef));
    }
    public static void Ret(Context ctx, Instr _)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ret));
    }
}