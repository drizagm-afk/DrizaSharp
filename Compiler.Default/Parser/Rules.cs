using DrzSharp.Compiler.Default.Lowerer;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

//===== ENTRY POINT =====
public class EntryPointRule : Rule<EntryPoint>
{
    const string BODY = "body";

    public EntryPointRule()
    {
        SetRealm(Realms.VIRTUAL);
        SetPattern(
            new TokenPattern()
                .THashPrefix("#RUN")
                .TKeyword("ASM")
                .TOpBrace()
                .ClosedGroup(captureTag: BODY)
                .TClBrace()
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
        Virtual.EntryPoint.New(ctx, NodeId);
        Virtual.InitASMMethod.New(ctx, NodeId, _labelCount);
        
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
    const string VARDECL = "varDecl";

    public ASMLocalsRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TOperator(".")
                .TKeyword("locals")
                .TOpBrace()
                .OptNEWLINE()
                .Repeat(t => t.Rule<ASMVarDeclRule>(captureTag: VARDECL)
                    .OptNEWLINE(), min: 0)
                .OptNEWLINE()
                .TClBrace()
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
    const string VARTYPE = "varType";
    const string VARNAME = "varName";

    public ASMVarDeclRule()
    {
        SetAbstract();
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TKeyword(captureTag: VARTYPE)
                .TKeyword(captureTag: VARNAME)
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
        Logic.NewLoc.New(ctx, NodeId);
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
    const string VARNAME = "varName";

    const string IsLDLOC = "ldloc";
    const string IsSTLOC = "stloc";

    public ASMVarUseRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .Or(
                    t => t.TKeyword("ldloc", IsLDLOC),
                    t => t.TKeyword("stloc", IsSTLOC)
                )
                .TKeyword(captureTag: VARNAME)
        );
    }

    protected override void OnInstantiate(MatchView view, ASMVarUse instance)
    {
        if (view.HasVar(IsLDLOC))
            instance.oper = VarOperations.LDLOC;
        if (view.HasVar(IsSTLOC))
            instance.oper = VarOperations.STLOC;

        instance._varName = view.LoadTokenVar(VARNAME);
    }
}
public class ASMVarUse : RuleInstance
{
    internal VarOperations oper;
    internal Token _varName;
    private int varId;

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
        if (oper == VarOperations.LDLOC)
            Logic.LdLoc.New(ctx, NodeId, varId);
        else
            Logic.StLoc.New(ctx, NodeId, varId);

        ctx.Emit();
    }
}
public enum VarOperations { LDLOC, STLOC }

//===== ASM Branches =====
public class ASMLabelRule : Rule<ASMLabel>
{
    const string LABELNAME = "labelName";

    public ASMLabelRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TKeyword(captureTag: LABELNAME)
                .TOperator(":")
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
        Logic.NewBr.New(ctx, NodeId, labelId);
        ctx.Emit();
    }
}

public class ASMBrRule : Rule<ASMBr>
{
    const string LABELNAME = "labelName";

    const string IsBR = "br";
    const string IsBRTRUE = "brtrue";
    const string IsBRFALSE = "brfalse";

    public ASMBrRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .Or(
                    t => t.TKeyword("br", IsBR),
                    t => t.TKeyword("brtrue", IsBRTRUE),
                    t => t.TKeyword("brfalse", IsBRFALSE)
                )
                .TKeyword(captureTag: LABELNAME)
        );
    }

    protected override void OnInstantiate(MatchView view, ASMBr instance)
    {
        if (view.HasVar(IsBR))
            instance.oper = BrOperation.BR;
        else if (view.HasVar(IsBRTRUE))
            instance.oper = BrOperation.BRTRUE;
        else if (view.HasVar(IsBRFALSE))
            instance.oper = BrOperation.BRFALSE;

        instance._labelName = view.LoadTokenVar(LABELNAME);
    }
}
public class ASMBr : RuleInstance
{
    internal BrOperation oper;
    internal Token _labelName;
    private int labelId;

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
        if (oper == BrOperation.BR)
            Logic.Br.New(ctx, NodeId, labelId);
        else if (oper == BrOperation.BRTRUE)
            Logic.BrTrue.New(ctx, NodeId, labelId);
        else
            Logic.BrFalse.New(ctx, NodeId, labelId);

        ctx.Emit();
    }
}
public enum BrOperation { BR, BRTRUE, BRFALSE }

//===== ASM Comparisons =====
public class ASMCompareRule : Rule<ASMCompare>
{
    const string IsCEQ = "ceq";
    const string IsCGT = "cgt";
    const string IsCLT = "clt";

    public ASMCompareRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .Or(
                    t => t.TKeyword("ceq", IsCEQ),
                    t => t.TKeyword("cgt", IsCGT),
                    t => t.TKeyword("clt", IsCLT)
                )
        );
    }
    protected override void OnInstantiate(MatchView view, ASMCompare instance)
    {
        if (view.HasVar(IsCEQ))
            instance.oper = CompOperation.CEQ;
        if (view.HasVar(IsCGT))
            instance.oper = CompOperation.CGT;
        if (view.HasVar(IsCLT))
            instance.oper = CompOperation.CLT;
    }
}
public class ASMCompare : RuleInstance
{
    internal CompOperation oper;

    protected override void OnEmit(EmitContext ctx)
    {
        if (oper == CompOperation.CEQ)
            Logic.Ceq.New(ctx, NodeId);
        else if (oper == CompOperation.CGT)
            Logic.Cgt.New(ctx, NodeId);
        else
            Logic.Clt.New(ctx, NodeId);

        ctx.Emit();
    }
}
public enum CompOperation { CEQ, CGT, CLT }

//===== ASM Arithmetic =====
public class ASMArithmeticRule : Rule<ASMArithmetic>
{
    const string IsADD = "add";
    const string IsSUB = "sub";
    const string IsMUL = "mul";
    const string IsDIV = "div";

    public ASMArithmeticRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .Or(
                    t => t.TKeyword("add", IsADD),
                    t => t.TKeyword("sub", IsSUB),
                    t => t.TKeyword("mul", IsMUL),
                    t => t.TKeyword("div", IsDIV)
                )
        );
    }

    protected override void OnInstantiate(MatchView view, ASMArithmetic instance)
    {
        if (view.HasVar(IsADD))
            instance.oper = AritOperation.ADD;
        if (view.HasVar(IsSUB))
            instance.oper = AritOperation.SUB;
        if (view.HasVar(IsMUL))
            instance.oper = AritOperation.MUL;
        if (view.HasVar(IsDIV))
            instance.oper = AritOperation.DIV;
    }
}
public class ASMArithmetic : RuleInstance
{
    internal AritOperation oper;

    protected override void OnEmit(EmitContext ctx)
    {
        if (oper == AritOperation.ADD)
            Logic.Add.New(ctx, NodeId);
        else if (oper == AritOperation.SUB)
            Logic.Sub.New(ctx, NodeId);
        else if (oper == AritOperation.MUL)
            Logic.Mul.New(ctx, NodeId);
        else
            Logic.Div.New(ctx, NodeId);

        ctx.Emit();
    }
}
public enum AritOperation { ADD, SUB, MUL, DIV }

//===== ASM Constants =====
public class ASMLdcI4Rule : Rule<ASMLdcI4>
{
    const string VALUE = "val";

    public ASMLdcI4Rule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TKeyword("ldc")
                .TKeyword("i4")
                .TNumber(captureTag: VALUE)
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
        int value = int.Parse(ctx.GetText(val.Id));

        Logic.LdcI4.New(ctx, NodeId, value);
        ctx.Emit();
    }
}

public class ASMLdstrRule : Rule<ASMLdstr>
{
    const string CONTENT = "cont";

    public ASMLdstrRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TKeyword("ldstr")
                .TString(captureTag: CONTENT)
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
        Logic.LdStr.New(ctx, NodeId, ctx.GetText(cont.Id));
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
                .TKeyword("print")
        );
    }
}
public class ASMPrint : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        Logic.Print.New(ctx, NodeId);
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
                .TKeyword("ret")
        );
    }
}
public class ASMReturn : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        Logic.Ret.New(ctx, NodeId);
        ctx.Emit();
    }
}