using DrzSharp.Compiler.Default.Lexer;
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
public class EntryPoint : RuleInstance
{
    internal TokenSpan _body;
    public int Body;

    protected override void OnBuild(BuildContext ctx)
    {
        Body = ctx.NestSpan(_body, Realms.ASMLogic);
    }
    protected override void OnEmit(EmitContext ctx)
    {
        Virtual.EntryPoint.New(ctx, NodeId);
        ctx.Emit(default, new EmitNode(0, Body));
    }
}

//===== ASM LOGIC =====
public class ASMLoadStrRule : Rule<ASMLoadStr>
{
    const string CONTENT = "cont";

    public ASMLoadStrRule()
    {
        SetRealm(Realms.ASMLogic);
        SetPattern(
            new TokenPattern()
                .TKeyword("ldstr")
                .TString(captureTag: CONTENT)
        );
    }
    protected override void OnInstantiate(MatchView view, ASMLoadStr instance)
    {
        instance.cont = view.LoadTokenVar(CONTENT);
    }
}
public class ASMLoadStr : RuleInstance
{
    internal Token cont;

    protected override void OnEmit(EmitContext ctx)
    {
        Logic.Ldstr.New(ctx, NodeId, ctx.GetText(cont.Id));
        ctx.Emit();
    }
}

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