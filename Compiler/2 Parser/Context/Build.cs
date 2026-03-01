using System.Diagnostics;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface BuildContext : Context, INodeAttrs
{
    public int NestSpan(TokenSpan span, int? realmId = null, bool isScoped = false);
    public bool TryNestSpan(TokenSpan span, out int nestId, int? realmId = null, bool isScoped = false);
    public int[] NestSpans(TokenSpan[] spans, int? realmId = null, bool isScoped = false);

    public void NestRule(RuleInstance inst, bool isScoped = false);
    public bool TryNestRule(RuleInstance? inst, bool isScoped = false);
    public void NestRules(RuleInstance[] insts, bool isScoped = false);
}

public partial class ParserProcess : BuildContext
{
    private int Nest(TokenSpan span, int? realmId = null, bool isScoped = false)
    {
        var slice = TAST.ToFlatSlice(span);
        var parentInfo = TAST.InfoAt(span.NodeId);

        return TAST.Nest(span.NodeId, slice.Start, slice.Length, new(realmId ?? parentInfo.RealmId, isScoped));
    }

    public int NestSpan(TokenSpan span, int? realmId = null, bool isScoped = false)
    {
        if (TAST.TryGetNest(span, out var nestId))
            return nestId;

        return Nest(span, realmId: realmId, isScoped: isScoped);
    }
    public bool TryNestSpan(TokenSpan span, out int nestId, int? realmId = null, bool isScoped = false)
    {
        nestId = 0;
        if (!span.IsValid) return false;

        nestId = NestSpan(span, realmId, isScoped);
        return true;
    }
    public int[] NestSpans(TokenSpan[] spans, int? realmId = null, bool isScoped = false)
    {
        int[] res = new int[spans.Length];
        for (int i = 0; i < spans.Length; i++)
            res[i] = NestSpan(spans[i], realmId, isScoped);

        return res;
    }

    public void NestRule(RuleInstance inst, bool isScoped = false)
    {
        var caller = RuleInst;

        //APPLY NESTING
        if (inst.NodeId < 0)
        {
            RuleInst = inst;
            inst.Build(this);

            inst.NodeId = Nest(inst.Span, isScoped: isScoped);
            TAST.ApplyRule(inst.NodeId, inst);
        }
        inst.Caller = RuleInst = caller;
    }
    public bool TryNestRule(RuleInstance? inst, bool isScoped = false)
    {
        if (inst is null) return false;

        NestRule(inst, isScoped);
        return true;
    }
    public void NestRules(RuleInstance[] insts, bool isScoped = false)
    {
        for (int i = 0; i < insts.Length; i++)
            NestRule(insts[i], isScoped);
    }
}