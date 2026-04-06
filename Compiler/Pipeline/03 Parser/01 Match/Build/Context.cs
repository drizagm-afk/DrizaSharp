using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface NestContext : Context
{
    public void SetScoped(bool isScoped = true);

    public int NestSpan(TokenSpan span, GlobalId? realmId = null, bool isScoped = false);
    public bool TryNestSpan(TokenSpan span, out int nestId, GlobalId? realmId = null, bool isScoped = false);
    public int[] NestSpans(TokenSpan[] spans, GlobalId? realmId = null, bool isScoped = false);

    public void NestRule(RuleInstance inst, bool isScoped = false);
    public bool TryNestRule(RuleInstance? inst, bool isScoped = false);
    public void NestRules(RuleInstance[] insts, bool isScoped = false);
}

public partial class ParserProcess : NestContext
{
    public void SetScoped(bool isScoped)
    => TAST.UpdateInfo(RuleInst!.NodeId, isScoped: isScoped);

    public partial int NestSpan(TokenSpan span, GlobalId? realmId, bool isScoped);
    public bool TryNestSpan(TokenSpan span, out int nestId, GlobalId? realmId = null, bool isScoped = false)
    {
        nestId = 0;
        if (!span.IsValid) return false;

        nestId = NestSpan(span, realmId, isScoped);
        return true;
    }
    public int[] NestSpans(TokenSpan[] spans, GlobalId? realmId, bool isScoped)
    {
        int[] res = new int[spans.Length];
        for (int i = 0; i < spans.Length; i++)
            res[i] = NestSpan(spans[i], realmId, isScoped);

        return res;
    }

    public partial void NestRule(RuleInstance inst, bool isScoped);
    public bool TryNestRule(RuleInstance? inst, bool isScoped)
    {
        if (inst is null) return false;

        NestRule(inst, isScoped);
        return true;
    }
    public void NestRules(RuleInstance[] insts, bool isScoped)
    {
        foreach (var inst in insts)
            NestRule(inst, isScoped);
    }
}

public interface BuildContext : Context, IAttrs { }
public partial class ParserProcess : BuildContext { }