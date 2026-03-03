using Mono.Cecil.Cil;

using EmitContext = DrzSharp.Compiler.Parser.EmitContext;
using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

public static partial class Logic
{
    //LOCALS
    public static int NewLoc_Id { get; internal set; }
    public static int NewLoc(EmitContext _) => NewLoc_Id;

    public static int LdLoc_Id { get; internal set; }
    public static int LdLoc(EmitContext ctx, int varId)
    {
        ctx.WriteInt(varId);
        return LdLoc_Id;
    }

    public static int StLoc_Id { get; internal set; }
    public static int StLoc(EmitContext ctx, int varId)
    {
        ctx.WriteInt(varId);
        return StLoc_Id;
    }

    //BRANCHES
    public static int NewBr_Id { get; internal set; }
    public static int NewBr(EmitContext ctx, int branchId)
    {
        ctx.WriteInt(branchId);
        return NewBr_Id;
    }

    public static int Br_Id { get; internal set; }
    public static int Br(EmitContext ctx, int branchId)
    {
        ctx.WriteInt(branchId);
        return Br_Id;
    }

    public static int BrTrue_Id { get; internal set; }
    public static int BrTrue(EmitContext ctx, int branchId)
    {
        ctx.WriteInt(branchId);
        return BrTrue_Id;
    }

    public static int BrFalse_Id { get; internal set; }
    public static int BrFalse(EmitContext ctx, int branchId)
    {
        ctx.WriteInt(branchId);
        return BrFalse_Id;
    }

    //COMPARISONS
    public static int Ceq_Id { get; internal set; }
    public static int Ceq(EmitContext _) => Ceq_Id;

    public static int Cgt_Id { get; internal set; }
    public static int Cgt(EmitContext _) => Cgt_Id;

    public static int Clt_Id { get; internal set; }
    public static int Clt(EmitContext _) => Clt_Id;

    //ARITHMETIC
    public static int Add_Id { get; internal set; }
    public static int Add(EmitContext _) => Add_Id;

    public static int Sub_Id { get; internal set; }
    public static int Sub(EmitContext _) => Sub_Id;

    public static int Mul_Id { get; internal set; }
    public static int Mul(EmitContext _) => Mul_Id;

    public static int Div_Id { get; internal set; }
    public static int Div(EmitContext _) => Div_Id;

    //CONSTANTS
    public static int LdcI4_Id { get; internal set; }
    public static int LdcI4(EmitContext ctx, int value)
    {
        ctx.WriteInt(value);
        return LdcI4_Id;
    }

    public static int LdStr_Id { get; internal set; }
    public static int LdStr(EmitContext ctx, string content)
    {
        ctx.WriteString(content);
        return LdStr_Id;
    }

    //METHODS
    public static int Print_Id { get; internal set; }
    public static int Print(EmitContext _) => Print_Id;

    public static int Ret_Id { get; internal set; }
    public static int Ret(EmitContext _) => Ret_Id;
}

public static partial class LogicRules
{
    //LOCALS
    public static void NewLoc(Context ctx)
    {
        ctx.Logic.Variables.Add(
            new(ctx.Module.TypeSystem.Int32)
        );
    }
    public static void LdLoc(Context ctx)
    {
        var localDef = ctx.Logic.Variables[ctx.ReadInt()];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldloc, localDef));
    }
    public static void StLoc(Context ctx)
    {
        var localDef = ctx.Logic.Variables[ctx.ReadInt()];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Stloc, localDef));
    }

    //BRANCHES
    public static void NewBr(Context ctx)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt()];
        var il = ctx.Logic.IL;

        il.Append(labelDef);
    }
    public static void Br(Context ctx)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt()];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Br, labelDef));
    }
    public static void BrTrue(Context ctx)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt()];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Brtrue, labelDef));
    }
    public static void BrFalse(Context ctx)
    {
        var labelDef = ctx.Logic.Labels[ctx.ReadInt()];

        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Brfalse, labelDef));
    }

    //COMPARISONS
    public static void Ceq(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ceq));
    }
    public static void Cgt(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Cgt));
    }
    public static void Clt(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Clt));
    }

    //ARITHMETIC
    public static void Add(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Add));
    }
    public static void Sub(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Sub));
    }
    public static void Mul(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Mul));
    }
    public static void Div(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Div));
    }

    //CONSTANTS
    public static void LdcI4(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldc_I4, ctx.ReadInt()));
    }
    public static void LdStr(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ldstr, ctx.ReadString()));
    }

    //METHODS
    public static void Print(Context ctx)
    {
        var il = ctx.Logic.IL;
        var writeLineRef = ctx.Module.ImportReference(
            typeof(Console).GetMethod("WriteLine", [typeof(int)])
        );
        il.Append(il.Create(OpCodes.Call, writeLineRef));
    }
    public static void Ret(Context ctx)
    {
        var il = ctx.Logic.IL;
        il.Append(il.Create(OpCodes.Ret));
    }
}