using System.Diagnostics;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface BuildContext : Context, INodeAttrs
{
    public int NestSpan(TokenSpan span, SchemeTASTArgs args = new());
    public bool TryNestSpan(TokenSpan span, out int nestId, SchemeTASTArgs args = new());
    public int[] NestSpans(TokenSpan[] spans, SchemeTASTArgs args = new());

    public void NestRule(RuleInstance inst, SchemeTASTArgs args = new());
    public bool TryNestRule(RuleInstance? inst, SchemeTASTArgs args = new());
    public void NestRules(RuleInstance[] insts, SchemeTASTArgs args = new());
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

    public int NestSpan(TokenSpan span, SchemeTASTArgs args = new())
    {
        bool isNestValid = IsNestValidAtSpan(span, out var hasNest, out int nestId);
        Debug.Assert(isNestValid);

        if (hasNest) return nestId;
        return Nest(span, args);
    }
    public bool TryNestSpan(TokenSpan span, out int nestId, SchemeTASTArgs args = new())
    {
        nestId = 0;
        if (!span.IsValid) return false;

        nestId = NestSpan(span, args);
        return true;
    }
    public int[] NestSpans(TokenSpan[] spans, SchemeTASTArgs args = new())
    {
        int[] res = new int[spans.Length];
        for (int i = 0; i < spans.Length; i++)
            res[i] = NestSpan(spans[i], args);

        return res;
    }

    public void NestRule(RuleInstance inst, SchemeTASTArgs args = new())
    {
        var caller = RuleInst;

        //APPLY NESTING
        if (inst.NodeId < 0)
        {
            RuleInst = inst;
            inst.Build(this);

            Debug.Assert(IsNestValidAtSpan(inst.Span, out _, out _));
            inst.NodeId = Nest(inst.Span, args);
            Site._ruleAppliance[inst.NodeId] = inst;
        }
        inst.Parent = RuleInst = caller;
    }
    public bool TryNestRule(RuleInstance? inst, SchemeTASTArgs args = new())
    {
        if (inst is null) return false;

        NestRule(inst, args);
        return true;
    }
    public void NestRules(RuleInstance[] insts, SchemeTASTArgs args = new())
    {
        for (int i = 0; i < insts.Length; i++)
            NestRule(insts[i], args);
    }
}