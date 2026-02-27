using System.Diagnostics;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface BuildContext : Context, INodeAttrs
{
    public int NestSpan(TokenSpan span, RealmId? realmId = null, bool? isScoped = null);
    public bool TryNestSpan(TokenSpan span, out int nestId, RealmId? realmId = null, bool? isScoped = null);
    public int[] NestSpans(TokenSpan[] spans, RealmId? realmId = null, bool? isScoped = null);

    public void NestRule(RuleInstance inst, bool? isScoped = null);
    public bool TryNestRule(RuleInstance? inst, bool? isScoped = null);
    public void NestRules(RuleInstance[] insts, bool? isScoped = null);
}

public partial class ParserProcess : BuildContext
{
    private int Nest(TokenSpan span, RealmId? realmId = null, bool? isScoped = null, int ruleId = -1)
    {
        var slice = TAST.ToFlatSlice(span);
        var args = TAST.ArgsAt(span.NodeId).With(realmId?.PhaseCode, realmId?.RealmCode, isScoped);

        return TAST.Nest(span.NodeId, slice.Start, slice.Length, new(args, ruleId));
    }

    public int NestSpan(TokenSpan span, RealmId? realmId = null, bool? isScoped = null)
    {
        if (TAST.TryGetNest(span, out var nestId))
            return nestId;

        return Nest(span, realmId: realmId, isScoped: isScoped);
    }
    public bool TryNestSpan(TokenSpan span, out int nestId, RealmId? realmId = null, bool? isScoped = null)
    {
        nestId = 0;
        if (!span.IsValid) return false;

        nestId = NestSpan(span, realmId, isScoped);
        return true;
    }
    public int[] NestSpans(TokenSpan[] spans, RealmId? realmId = null, bool? isScoped = null)
    {
        int[] res = new int[spans.Length];
        for (int i = 0; i < spans.Length; i++)
            res[i] = NestSpan(spans[i], realmId, isScoped);

        return res;
    }

    public void NestRule(RuleInstance inst, bool? isScoped = null)
    {
        var caller = RuleInst;

        //APPLY NESTING
        if (inst.NodeId < 0)
        {
            RuleInst = inst;
            inst.Build(this);

            inst.NodeId = Nest(inst.Span, isScoped: isScoped, ruleId: inst.RuleId);
            Site._ruleAppliance[inst.NodeId] = inst;
        }
        inst.Parent = RuleInst = caller;
    }
    public bool TryNestRule(RuleInstance? inst, bool? isScoped = null)
    {
        if (inst is null) return false;

        NestRule(inst, isScoped);
        return true;
    }
    public void NestRules(RuleInstance[] insts, bool? isScoped = null)
    {
        for (int i = 0; i < insts.Length; i++)
            NestRule(insts[i], isScoped);
    }
}