using DrzSharp.Compiler.Default.Lowerer;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

//===== ENTRY POINT =====
public class EntryPointRule : Rule<EntryPoint>
{
    const int BODY = 0;

    public EntryPointRule()
    {
        SetRealm(Realms.VIRTUAL);
        SetPattern(
            new TokenPattern()
                .hashpx("#RUN").kw("ASM").obrace().CGroup(captureTag: BODY).cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, EntryPoint instance)
    {
        instance._body = view.LoadVar(BODY);
    }
}
public class EntryPoint : RuleInstance, IASMMethod
{
    internal TokenSpan _body;
    public int Body;

    protected override void OnBuild(BuildContext ctx)
    {
        Body = ctx.NestSpan(_body, Realms.ASMLogic);
    }

    private int _varCount = 0;
    public int NewVar() => _varCount++;

    private int _labelCount = 0;
    private Dictionary<string, int> _labels = [];
    public int NewLabel() => _labelCount++;
    public int NewLabel(string name)
    {
        if (_labels.TryGetValue(name, out int id))
            return id;

        return _labels[name] = NewLabel();
    }

    protected override void OnValidate(ValidateContext ctx)
    {
        ctx.StoreTag(Tags.MethodBody, "");
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Virtual.EntryPoint(ctx));
        ctx.EmitInstr(Virtual.InitASMMethod(ctx, _labelCount));

        ctx.Emit(default, new EmitNode(1, Body));
    }
}

public interface IMethod
{
    public int NewVar();
    public int NewLabel();
}
public interface IASMMethod : IMethod
{
    public int NewLabel(string name);
}

//===== ASM Locals =====
public class ASMLocalsRule : Rule<ASMLocals>
{
    const int VARDECL = 0;

    public ASMLocalsRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .oper(".").kw("locals").obrace()
                .OptNl().Repeat(t =>
                    t.Rule<ASMVarDeclRule>(VARDECL).OptNl()
                ).OptNl()
                .cbrace()
        );
    }

    protected override void OnInstantiate(MatchView view, ASMLocals instance)
    {
        instance.vars = view.LoadRuleVars<ASMVarDecl>(VARDECL);
    }
}
public class ASMLocals : RuleInstance
{
    internal ASMVarDecl[] vars = null!;
    public ASMLocals() { BypassEmit = true; }

    protected override void OnBuild(BuildContext ctx)
    {
        ctx.NestRules(vars);
    }
}

public class ASMVarDeclRule : Rule<ASMVarDecl>
{
    const int VARTYPE = 0;
    const int VARNAME = 1;

    public ASMVarDeclRule()
    {
        SetAbstract();
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw(captureTag: VARTYPE)
                .kw(captureTag: VARNAME)
        );
    }

    protected override void OnInstantiate(MatchView view, ASMVarDecl instance)
    {
        instance.varName = view.LoadTokenVar(VARNAME);
        instance.varType = view.LoadTokenVar(VARTYPE);
    }
}
public class ASMVarDecl : RuleInstance, IVarDecl
{
    internal Token varType;
    internal Token varName;

    public int VarId { get; private set; }
    public string VarName { get; private set; } = "";
    public string VarType { get; private set; } = "";

    protected override void OnValidate(ValidateContext ctx)
    {
        VarName = ctx.GetText(varName.Id);
        VarType = ctx.GetText(varType.Id);

        if (!ctx.TryResolveTag(Tags.MethodBody, "", out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        if (ctx.HasTag(Tags.VarDecl, VarName))
            ctx.Abort($"The Var {VarName} is created more than once");

        //STORING VAR DECL TAG
        var method = (IMethod)methodInst;
        VarId = method.NewVar();

        ctx.StoreTag(Tags.VarDecl, VarName);
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.NewLoc(ctx));
        ctx.Emit();
    }
}
public interface IVarDecl
{
    public int VarId { get; }
    public string VarName { get; }
    public string VarType { get; }
}

public class ASMVarUseRule : Rule<ASMVarUse>
{
    const int VARNAME = 10;

    public ASMVarUseRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .KeywordTable("ldloc", "stloc")
                .kw(captureTag: VARNAME)
        );
    }

    protected override void OnInstantiate(MatchView view, ASMVarUse instance)
    {
        view.HasVarInRange(0..2, out instance.oper);
        instance._varName = view.LoadTokenVar(VARNAME);
    }
}
public class ASMVarUse : RuleInstance
{
    internal Token _varName;
    private int varId;

    const int LDLOC = 0;
    const int STLOC = 1;
    internal int oper;

    protected override void OnValidate(ValidateContext ctx)
    {
        var varName = ctx.GetText(_varName.Id);

        if (!ctx.TryResolveTag(Tags.VarDecl, varName, out var declInst))
            ctx.Abort($"The Var {varName} is used before being created");

        //GETTING VAR DECL ID
        var decl = (IVarDecl)declInst;
        varId = decl.VarId;
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(
            oper switch
            {
                LDLOC => Logic.LdLoc(ctx, varId),
                _ => Logic.StLoc(ctx, varId),
            }
        );
        ctx.Emit();
    }
}

//===== ASM Branches =====
public class ASMLabelRule : Rule<ASMLabel>
{
    const int LABELNAME = 0;

    public ASMLabelRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw(captureTag: LABELNAME)
                .oper(":")
        );
    }

    protected override void OnInstantiate(MatchView view, ASMLabel instance)
    {
        instance._labelName = view.LoadTokenVar(LABELNAME);
    }
}
public class ASMLabel : RuleInstance
{
    internal Token _labelName;
    private string labelName = "";
    private int labelId;

    protected override void OnValidate(ValidateContext ctx)
    {
        labelName = ctx.GetText(_labelName.Id);

        if (!ctx.TryResolveTag(Tags.MethodBody, "", out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        //STORING VAR DECL TAG
        var method = (IASMMethod)methodInst;
        labelId = method.NewLabel(labelName);
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.NewBr(ctx, labelId));
        ctx.Emit();
    }
}

public class ASMBrRule : Rule<ASMBr>
{
    const int LABELNAME = 10;

    public ASMBrRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .KeywordTable("br", "brtrue", "brfalse")
                .kw(captureTag: LABELNAME)
        );
    }

    protected override void OnInstantiate(MatchView view, ASMBr instance)
    {
        view.HasVarInRange(0..4, out instance.oper);
        instance._labelName = view.LoadTokenVar(LABELNAME);
    }
}
public class ASMBr : RuleInstance
{
    internal Token _labelName;
    private int labelId;

    const int BR = 0;
    const int BRTRUE = 1;
    const int BRFALSE = 2;

    internal int oper;

    protected override void OnValidate(ValidateContext ctx)
    {
        var labelName = ctx.GetText(_labelName.Id);

        if (!ctx.TryResolveTag(Tags.MethodBody, "", out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        //LOADING LABEL DECL TAG
        var method = (IASMMethod)methodInst;
        labelId = method.NewLabel(labelName);
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(
            oper switch
            {
                BR => Logic.Br(ctx, labelId),
                BRTRUE => Logic.Br(ctx, labelId),
                _ => Logic.BrFalse(ctx, labelId)
            }
        );
        ctx.Emit();
    }
}

//===== ASM Comparisons =====
public class ASMCompareRule : Rule<ASMCompare>
{
    public ASMCompareRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .KeywordTable("ceq", "cgt", "clt")
        );
    }
    protected override void OnInstantiate(MatchView view, ASMCompare instance)
    {
        view.HasVarInRange(0..3, out instance.oper);
    }
}
public class ASMCompare : RuleInstance
{
    const int CEQ = 0;
    const int CGT = 1;
    const int CLT = 2;

    internal int oper;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(
            oper switch
            {
                CEQ => Logic.Ceq(ctx),
                CGT => Logic.Cgt(ctx),
                _ => Logic.Clt(ctx)
            }
        );
        ctx.Emit();
    }
}

//===== ASM Arithmetic =====
public class ASMArithmeticRule : Rule<ASMArithmetic>
{
    public ASMArithmeticRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .KeywordTable("add", "sub", "mul", "div")
        );
    }

    protected override void OnInstantiate(MatchView view, ASMArithmetic instance)
    {
        view.HasVarInRange(0..4, out instance.oper);
    }
}
public class ASMArithmetic : RuleInstance
{
    const int ADD = 0;
    const int SUB = 1;
    const int MUL = 2;
    const int DIV = 3;

    internal int oper;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(
            oper switch
            {
                ADD => Logic.Add(ctx),
                SUB => Logic.Sub(ctx),
                MUL => Logic.Mul(ctx),
                _ => Logic.Div(ctx)
            }
        );
        ctx.Emit();
    }
}

//===== ASM Constants =====
public class ASMLdcI4Rule : Rule<ASMLdcI4>
{
    const int VALUE = 0;

    public ASMLdcI4Rule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw("ldc")
                .kw("i4")
                .numberVal(captureTag: VALUE)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLdcI4 instance)
    {
        instance.val = view.LoadTokenVar(VALUE);
    }
}
public class ASMLdcI4 : RuleInstance
{
    internal Token val;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.LdcI4(
            ctx, int.Parse(ctx.GetText(val.Id))
        ));
        ctx.Emit();
    }
}

public class ASMLdstrRule : Rule<ASMLdstr>
{
    const int CONTENT = 0;

    public ASMLdstrRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw("ldstr")
                .stringVal(captureTag: CONTENT)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLdstr instance)
    {
        instance.cont = view.LoadTokenVar(CONTENT);
    }
}
public class ASMLdstr : RuleInstance
{
    internal Token cont;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.LdStr(
            ctx, ctx.GetText(cont.Id)
        ));
        ctx.Emit();
    }
}

//===== ASM Methods =====
public class ASMPrintRule : Rule<ASMPrint>
{
    public ASMPrintRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw("print")
        );
    }
}
public class ASMPrint : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.Print(ctx));
        ctx.Emit();
    }
}

public class ASMReturnRule : Rule<ASMReturn>
{
    public ASMReturnRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .kw("ret")
        );
    }
}
public class ASMReturn : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.EmitInstr(Logic.Ret(ctx));
        ctx.Emit();
    }
}