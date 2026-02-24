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
    private int Nest(TokenSpan span, SchemeTASTArgs args)
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
                if (start < 0) (start, rem) = (i, node.Length);
                else break;
            }

            if (childExists && child.RelStart == i)
            {
                i += child.RelLength;
                rem -= child.Length;
                continue;
            }

            i++;
            rem--;
        }
        return TAST.Nest(span.NodeId, start, i - start, args);
    }

    public int NestSpan(TokenSpan span, RealmId? realmId = null, bool? isScoped = null)
    {
        SchemeTASTArgs args = new(realmId?.PhaseCode, realmId?.RealmCode, isScoped);

        bool isNestValid = IsNestValidAtSpan(span, out var hasNest, out int nestId);
        Debug.Assert(isNestValid);

        if (hasNest) return nestId;
        return Nest(span, args);
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

            Debug.Assert(IsNestValidAtSpan(inst.Span, out _, out _));
            inst.NodeId = Nest(inst.Span, new(isScoped: isScoped));
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