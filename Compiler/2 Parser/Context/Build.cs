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
    private int Nest(TokenSpan span, TASTArgs args, int ruleId = -1)
    {
        ref readonly var node = ref TAST.NodeAt(span.NodeId);
        int start = -1;

        //EVAL
        int i = TAST.SkipOffset(node, span.Offset, out var childExists, out var child);
        int rem = span.Start;
        while (i < node.Length)
        {
            if (rem <= 0)
            {
                if (start < 0) (start, rem) = (i, span.Length);
                else break;
            }

            if (childExists && child.RelStart == i)
            {
                i += child.RelLength;
                rem -= child.Length;
                childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                continue;
            }

            i++;
            rem--;
        }
        return TAST.Nest(span.NodeId, start, i - start, new(args, default, ruleId));
    }

    public int NestSpan(TokenSpan span, RealmId? realmId = null, bool? isScoped = null)
    {
        bool isNestValid = IsNestValidAtSpan(span, out var hasNest, out int nestId);
        Debug.Assert(isNestValid);

        if (hasNest) return nestId;

        return Nest(
            span,
            TAST.ArgsAt(span.NodeId).With(realmId?.PhaseCode, realmId?.RealmCode, isScoped)
        );
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

            var span = inst.Span;
            Debug.Assert(IsNestValidAtSpan(span, out _, out _));

            inst.NodeId = Nest(
                span, 
                TAST.ArgsAt(span.NodeId).With(isScoped: isScoped), inst.RuleId
            );
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