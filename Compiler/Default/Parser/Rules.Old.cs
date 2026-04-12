/*
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
            .hashPx("#Run").obrace().Body(BODY).cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, EntryPoint inst)
    {
        inst._body = view.LoadVar(BODY);
    }
}
public class EntryPoint : RuleInstance
{
    internal TokenSpan _body;
    protected override void OnNest(NestContext ctx)
    {
        Body = ctx.NestSpan(_body, Realms.Logic);
    }

    public int Body { get; private set; }

    private int _labelCount = 0;
    public int AddLabel() => _labelCount++;
    private int _localCount = 0;
    public int AddLocal() => _localCount++;
    protected override void OnValidate(ValidateContext ctx)
    {
        ctx.StoreTag(Tags.MethodBody);
    }

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(Temporal.EnterMethod(ctx, _labelCount, _localCount));
        ctx.AddInnerEmit(Body);
        ctx.AddInstr(Flow.Return(ctx));
        ctx.Emit();
    }
}

//===== LOGIC =====
public class VarDeclRule : Rule<VarDecl>
{
    const int VARNAME = 0;
    const int EXPR = 1;
    public VarDeclRule()
    {
        SetPattern(t => t
            .kw(captureTag: VARNAME).oper(":=").RuleClass<ExprRule>(EXPR)
        );
    }
    protected override void OnInstantiate(MatchView view, VarDecl inst)
    {
        inst._varName = view.LoadTokenVar(VARNAME);
        inst.Expr = view.LoadRuleVar<Expr>(EXPR);
    }
}
public class VarDecl : RuleInstance
{
    internal Token _varName;
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Expr);
    }

    public Expr Expr { get; internal set; } = null!;

    public int VarId { get; private set; }
    public string VarName { get; private set; } = null!;
    protected override void OnValidate(ValidateContext ctx)
    {
        var entryPoint = ctx.ResolveTag<EntryPoint>(Tags.MethodBody);
        VarId = entryPoint.AddLocal();
        VarName = ctx.GetText(_varName.Id);

        ctx.StoreTag(Tags.VarDecl, VarName);
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(Local.Declare(ctx, VarId));
        ctx.AddInnerEmit(Expr.NodeId);
        ctx.AddInstr(Local.Store(ctx, VarId));
        ctx.Emit();
    }
}

public class IfStmtRule : Rule<IfStmt>
{
    const int EXPR = 0;
    const int BODY = 1;
    public IfStmtRule()
    {
        SetPattern(t => t
            .kw("if").oparen().RuleClass<ExprRule>(EXPR).cparen().obrace().Body(BODY).cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, IfStmt inst)
    {
        inst.Expr = view.LoadRuleVar<Expr>(EXPR);
        inst._body = view.LoadVar(BODY);
    }
}
public class IfStmt : RuleInstance
{
    internal TokenSpan _body;
    protected override void OnNest(NestContext ctx)
    {
        Body = ctx.NestSpan(_body);
        ctx.NestRule(Expr);
    }

    public Expr Expr { get; internal set; } = null!;
    public int Body { get; private set; }

    private int _labelEnd;
    protected override void OnValidate(ValidateContext ctx)
    {
        var entryPoint = ctx.ResolveTag<EntryPoint>(Tags.MethodBody);
        _labelEnd = entryPoint.AddLabel();
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Expr.NodeId);
        ctx.AddInstr(Branch.GotoIfFalse(ctx, _labelEnd));
        ctx.AddInnerEmit(Body);
        ctx.AddInstr(Branch.Label(ctx, _labelEnd));
        ctx.Emit();
    }
}

public class RepeatStmtRule : Rule<RepeatStmt>
{
    const int EXPR = 0;
    const int BODY = 1;
    public RepeatStmtRule()
    {
        SetPattern(t => t
            .kw("repeat").oparen().RuleClass<ExprRule>(EXPR).cparen().obrace().Body(BODY).cbrace()
        );
    }
    protected override void OnInstantiate(MatchView view, RepeatStmt inst)
    {
        inst.Expr = view.LoadRuleVar<Expr>(EXPR);
        inst._body = view.LoadVar(BODY);
    }
}
public class RepeatStmt : RuleInstance
{
    internal TokenSpan _body;
    protected override void OnNest(NestContext ctx)
    {
        Body = ctx.NestSpan(_body);
        ctx.NestRule(Expr);
    }

    public Expr Expr { get; internal set; } = null!;
    public int Body { get; private set; }

    private int _varCount;
    private int _labelStart;
    private int _labelEnd;
    protected override void OnValidate(ValidateContext ctx)
    {
        var entryPoint = ctx.ResolveTag<EntryPoint>(Tags.MethodBody);
        _varCount = entryPoint.AddLocal();
        _labelStart = entryPoint.AddLabel();
        _labelEnd = entryPoint.AddLabel();
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(Local.Declare(ctx, _varCount));
        ctx.AddInnerEmit(Expr.NodeId);
        ctx.AddInstr(Local.Store(ctx, _varCount));

        ctx.AddInstr(Branch.Label(ctx, _labelStart));
        ctx.AddInstr(Local.Load(ctx, _varCount));
        ctx.AddInstr(Const.Int32(ctx, 0));
        ctx.AddInstr(Compare.GreaterThan(ctx));
        ctx.AddInstr(Branch.GotoIfFalse(ctx, _labelEnd));
        ctx.AddInnerEmit(Body);

        ctx.AddInstr(Local.Load(ctx, _varCount));
        ctx.AddInstr(Const.Int32(ctx, 1));
        ctx.AddInstr(Arith.Sub(ctx));
        ctx.AddInstr(Local.Store(ctx, _varCount));
        ctx.AddInstr(Branch.Goto(ctx, _labelStart));
        ctx.AddInstr(Branch.Label(ctx, _labelEnd));

        ctx.Emit();
    }
}

public class VarSetRule : Rule<VarSet>
{
    const int VARNAME = 0;
    const int EXPR = 1;
    public VarSetRule()
    {
        SetPattern(t => t
            .kw(captureTag: VARNAME).oper("=").RuleClass<ExprRule>(EXPR)
        );
    }
    protected override void OnInstantiate(MatchView view, VarSet inst)
    {
        inst._varName = view.LoadTokenVar(VARNAME);
        inst.Expr = view.LoadRuleVar<Expr>(EXPR);
    }
}
public class VarSet : RuleInstance
{
    internal Token _varName;
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Expr);
    }

    public Expr Expr { get; internal set; } = null!;

    private int _varId;
    protected override void OnValidate(ValidateContext ctx)
    {
        var decl = ctx.ResolveTag<VarDecl>(Tags.VarDecl, ctx.GetText(_varName.Id));
        _varId = decl.VarId;
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Expr.NodeId);
        ctx.AddInstr(Local.Store(ctx, _varId));
        ctx.Emit();
    }
}

public class PrintRule : Rule<Print>
{
    const int EXPR = 0;
    public PrintRule()
    {
        SetPattern(t => t
            .kw("print").oparen().RuleClass<ExprRule>(EXPR).cparen()
        );
    }
    protected override void OnInstantiate(MatchView view, Print inst)
    {
        inst.Expr = view.LoadRuleVar<Expr>(EXPR);
    }
}
public class Print : RuleInstance
{
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Expr);
    }

    public Expr Expr { get; internal set; } = null!;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Expr.NodeId);
        ctx.AddInstr(Temporal.Print(ctx));
        ctx.Emit();
    }
}

//===== EXPRESSIONS =====
public class ExprRule : RuleClass<Expr> { }
public class Expr : RuleInstance { }

public class MonoExprRule : RuleClass<MonoExpr> { }
public class MonoExpr : Expr { }

public class ChainExprRule : RuleClass<ChainExpr> { }
public class ChainExpr : Expr { }

//===== MONO EXPRESSIONS =====
public class VarGetRule : Rule<VarGet>
{
    const int VARNAME = 0;
    public VarGetRule()
    {
        SetPattern(t => t
            .kw(captureTag: VARNAME)
        );
    }
    protected override void OnInstantiate(MatchView view, VarGet inst)
    {
        inst._varName = view.LoadTokenVar(VARNAME);
    }
}
public class VarGet : MonoExpr
{
    internal Token _varName;

    private int _varId;
    protected override void OnValidate(ValidateContext ctx)
    {
        var decl = ctx.ResolveTag<VarDecl>(Tags.VarDecl, ctx.GetText(_varName.Id));
        _varId = decl.VarId;
    }
    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(Local.Load(ctx, _varId));
        ctx.Emit();
    }
}

public class NumberLitRule : Rule<NumberLit>
{
    const int VALUE = 0;
    public NumberLitRule()
    {
        SetPattern(t => t
            .numberLit(captureTag: VALUE)
        );
    }
    protected override void OnInstantiate(MatchView view, NumberLit inst)
    {
        inst._value = view.LoadTokenVar(VALUE);
    }
}
public class NumberLit : MonoExpr
{
    internal Token _value;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(Const.Int32(ctx, int.Parse(ctx.GetText(_value.Id))));
        ctx.Emit();
    }
}

//===== CHAIN EXPRESSIONS =====
public class AddExprRule : Rule<AddExpr>
{
    const int LEFT = 0;
    const int RIGHT = 1;
    public AddExprRule()
    {
        SetPattern(t => t
            .RuleClass<MonoExprRule>(LEFT).oper("+").RuleClass<MonoExprRule>(RIGHT)
        );
    }
    protected override void OnInstantiate(MatchView view, AddExpr inst)
    {
        inst.Left = view.LoadRuleVar<MonoExpr>(LEFT);
        inst.Right = view.LoadRuleVar<MonoExpr>(RIGHT);
    }
}
public class AddExpr : ChainExpr
{
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Left);
        ctx.NestRule(Right);
    }

    public MonoExpr Left { get; internal set; } = null!;
    public MonoExpr Right { get; internal set; } = null!;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Left.NodeId);
        ctx.AddInnerEmit(Right.NodeId);
        ctx.AddInstr(Arith.Add(ctx));
        ctx.Emit();
    }
}

public class SubExprRule : Rule<SubExpr>
{
    const int LEFT = 0;
    const int RIGHT = 1;
    public SubExprRule()
    {
        SetPattern(t => t
            .RuleClass<MonoExprRule>(LEFT).oper("-").RuleClass<MonoExprRule>(RIGHT)
        );
    }
    protected override void OnInstantiate(MatchView view, SubExpr inst)
    {
        inst.Left = view.LoadRuleVar<MonoExpr>(LEFT);
        inst.Right = view.LoadRuleVar<MonoExpr>(RIGHT);
    }
}
public class SubExpr : ChainExpr
{
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Left);
        ctx.NestRule(Right);
    }

    public MonoExpr Left { get; internal set; } = null!;
    public MonoExpr Right { get; internal set; } = null!;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Left.NodeId);
        ctx.AddInnerEmit(Right.NodeId);
        ctx.AddInstr(Arith.Sub(ctx));
        ctx.Emit();
    }
}

public class MulExprRule : Rule<MulExpr>
{
    const int LEFT = 0;
    const int RIGHT = 1;
    public MulExprRule()
    {
        SetPattern(t => t
            .RuleClass<MonoExprRule>(LEFT).oper("*").RuleClass<MonoExprRule>(RIGHT)
        );
    }
    protected override void OnInstantiate(MatchView view, MulExpr inst)
    {
        inst.Left = view.LoadRuleVar<MonoExpr>(LEFT);
        inst.Right = view.LoadRuleVar<MonoExpr>(RIGHT);
    }
}
public class MulExpr : ChainExpr
{
    protected override void OnNest(NestContext ctx)
    {
        ctx.NestRule(Left);
        ctx.NestRule(Right);
    }

    public MonoExpr Left { get; internal set; } = null!;
    public MonoExpr Right { get; internal set; } = null!;

    protected override void OnEmit(EmitContext ctx)
    {
        ctx.AddInstr(InstrType.None);
        ctx.AddInnerEmit(Left.NodeId);
        ctx.AddInnerEmit(Right.NodeId);
        ctx.AddInstr(Arith.Mul(ctx));
        ctx.Emit();
    }
}
*/