using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Default.Patterns;
using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Parser;

//===== ENTRY POINT =====
public class EntryPointRule : Rule<EntryPoint>
{
    const int BODY = 0;
    public EntryPointRule()
    {
        SetPattern(t => t
            .hashPx("#RUN").kw("ASM").obrace().CGroup(BODY).cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, EntryPoint inst)
    {
        inst._body = view.LoadVar(BODY);
    }
}
public class EntryPoint : RuleInstance, IASMMethod
{
    internal TokenSpan _body;
    public int Body;

    protected override void OnNest(NestContext ctx)
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
        ctx.StoreTag(Tags.MethodBody);
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.EnterMethod(_labelCount, _varCount));
        ctx.AddInnerEmit(Body);
        ctx.Emit();
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
        SetPattern(t => t
            .oper(".").kw("locals").obrace()
            .OptNl().Repeat(t =>
                t.Rule<ASMVarDeclRule>(VARDECL).OptNl()
            ).OptNl()
            .cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLocals inst)
    {
        inst.vars = view.LoadRuleVars<ASMVarDecl>(VARDECL);
    }
}
public class ASMLocals : RuleInstance
{
    internal ASMVarDecl[] vars = null!;
    public ASMLocals() { BypassEmit = true; }

    protected override void OnNest(NestContext ctx)
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
        SetPattern(t => t
            .kw(captureTag: VARTYPE)
            .kw(captureTag: VARNAME)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMVarDecl inst)
    {
        inst.varName = view.LoadTokenVar(VARNAME);
        inst.varType = view.LoadTokenVar(VARTYPE);
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

        if (!ctx.TryResolveTag(Tags.MethodBody, out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        if (ctx.HasTag(Tags.VarDecl, VarName))
            ctx.Abort($"The Var \"{VarName}\" is created more than once");

        //STORING VAR DECL TAG
        var method = (IMethod)methodInst;
        VarId = method.NewVar();

        ctx.StoreTag(Tags.VarDecl, VarName);
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.DeclLocal(VarId));
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
    const int TABLE = 0;
    const int VARNAME = 1;
    public ASMVarUseRule()
    {
        SetPattern(t => t
            .KwTable(TABLE, "ldloc", "stloc")
            .kw(captureTag: VARNAME)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMVarUse inst)
    {
        inst.oper = view.LoadTableVar(TABLE);
        inst._varName = view.LoadTokenVar(VARNAME);
    }
}
public class ASMVarUse : RuleInstance
{
    internal Token _varName;
    private int varId;

    const byte LDLOC = 0;
    const byte STLOC = 1;
    internal byte oper;

    protected override void OnValidate(ValidateContext ctx)
    {
        var varName = ctx.GetText(_varName.Id);

        if (!ctx.TryResolveTag(Tags.VarDecl, varName, out var declInst))
            ctx.Abort($"The Var \"{varName}\" is used before being created");

        //GETTING VAR DECL ID
        var decl = (IVarDecl)declInst;
        varId = decl.VarId;
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(
            oper switch
            {
                LDLOC => ctx.LoadLocal(varId),
                _ => ctx.StoreLocal(varId),
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
        SetPattern(t => t
            .kw(captureTag: LABELNAME)
            .oper(":")
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLabel inst)
    {
        inst._labelName = view.LoadTokenVar(LABELNAME);
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

        if (!ctx.TryResolveTag(Tags.MethodBody, out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        //STORING VAR DECL TAG
        var method = (IASMMethod)methodInst;
        labelId = method.NewLabel(labelName);
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.Label(labelId));
        ctx.Emit();
    }
}

public class ASMBrRule : Rule<ASMBr>
{
    const int TABLE = 0;
    const int LABELNAME = 1;
    public ASMBrRule()
    {
        SetPattern(t => t
            .KwTable(TABLE, "br", "brtrue", "brfalse")
            .kw(captureTag: LABELNAME)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMBr inst)
    {
        inst.oper = view.LoadTableVar(TABLE);
        inst._labelName = view.LoadTokenVar(LABELNAME);
    }
}
public class ASMBr : RuleInstance
{
    internal Token _labelName;
    private int labelId;

    const byte BR = 0;
    const byte BRTRUE = 1;
    const byte BRFALSE = 2;
    internal byte oper;

    protected override void OnValidate(ValidateContext ctx)
    {
        var labelName = ctx.GetText(_labelName.Id);

        if (!ctx.TryResolveTag(Tags.MethodBody, out var methodInst))
            ctx.Abort($"The current ENVIRONMENT isn't a METHOD BODY");

        //LOADING LABEL DECL TAG
        var method = (IASMMethod)methodInst;
        labelId = method.NewLabel(labelName);
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(
            oper switch
            {
                BR => ctx.Br(labelId),
                BRTRUE => ctx.BrIfTrue(labelId),
                _ => ctx.BrIfFalse(labelId)
            }
        );
        ctx.Emit();
    }
}

//===== ASM Comparisons =====
public class ASMCompareRule : Rule<ASMCompare>
{
    const int TABLE = 0;
    public ASMCompareRule()
    {
        SetPattern(t => t
            .KwTable(TABLE, "ceq", "cgt", "clt")
        );
    }
    protected override void OnInstantiate(MatchView view, ASMCompare inst)
    {
        inst.oper = view.LoadTableVar(TABLE);
    }
}
public class ASMCompare : RuleInstance
{
    const byte CEQ = 0;
    const byte CGT = 1;
    const byte CLT = 2;
    internal byte oper;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(
            oper switch
            {
                CEQ => ctx.Equal(),
                CGT => ctx.GreaterThan(),
                _ => ctx.LessThan()
            }
        );
        ctx.Emit();
    }
}

//===== ASM Arithmetic =====
public class ASMArithmeticRule : Rule<ASMArithmetic>
{
    const int TABLE = 0;
    public ASMArithmeticRule()
    {
        SetPattern(t => t
            .KwTable(TABLE, "add", "sub", "mul", "div")
        );
    }
    protected override void OnInstantiate(MatchView view, ASMArithmetic inst)
    {
        inst.oper = view.LoadTableVar(TABLE);
    }
}
public class ASMArithmetic : RuleInstance
{
    const byte ADD = 0;
    const byte SUB = 1;
    const byte MUL = 2;
    const byte DIV = 3;
    internal byte oper;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(
            oper switch
            {
                ADD => ctx.Add(),
                SUB => ctx.Sub(),
                MUL => ctx.Mul(),
                _ => ctx.Div()
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
        SetPattern(t => t
            .kw("ldc")
            .oper(".")
            .kw("i4")
            .numberLit(captureTag: VALUE)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLdcI4 inst)
    {
        inst.val = view.LoadTokenVar(VALUE);
    }
}
public class ASMLdcI4 : RuleInstance
{
    internal Token val;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.Int32(int.Parse(ctx.GetText(val.Id))));
        ctx.Emit();
    }
}

public class ASMLdstrRule : Rule<ASMLdstr>
{
    const int VALUE = 0;
    public ASMLdstrRule()
    {
        SetPattern(t => t
            .kw("ldstr")
            .stringLit(captureTag: VALUE)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLdstr inst)
    {
        inst.val = view.LoadTokenVar(VALUE);
    }
}
public class ASMLdstr : RuleInstance
{
    internal Token val;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.String(ctx.GetText(val.Id)));
        ctx.Emit();
    }
}

//===== ASM Methods =====
public class ASMPrintRule : Rule<ASMPrint>
{
    public ASMPrintRule()
    {
        SetPattern(t => t
            .kw("print")
        );
    }
}
public class ASMPrint : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.Print());
        ctx.Emit();
    }
}

public class ASMReturnRule : Rule<ASMReturn>
{
    public ASMReturnRule()
    {
        SetPattern(t => t
            .kw("ret")
        );
    }
}
public class ASMReturn : RuleInstance
{
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(ctx.Return());
        ctx.Emit();
    }
}