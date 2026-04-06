using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    private void Build(RuleInstance inst)
    {
        NestRule(inst, false);
        RuleInst = null;
    }

    private int Nest(TokenSpan span, int? realmId, bool isScoped)
    {
        var slice = TAST.ToFlatSlice(span);
        var parentInfo = TAST.InfoAt(span.NodeId);

        return TAST.Nest(span.NodeId, slice.Start, slice.Length, new(realmId ?? parentInfo.RealmId, isScoped));
    }
    public partial int NestSpan(TokenSpan span, int? realmId, bool isScoped)
    {
        if (TAST.TryGetNest(span, out var nestId))
            return nestId;

        return Nest(span, realmId: realmId, isScoped: isScoped);
    }
    public partial void NestRule(RuleInstance inst, bool isScoped)
    {
        var caller = RuleInst;

        if (inst.NodeId < 0)
        {
            RuleInst = inst;
            inst.Nest(this);

            inst.NodeId = Nest(inst.Span, GetRule(inst.RuleId).RealmId, isScoped);
            TAST.ApplyRule(inst.NodeId, inst);
        }
        inst.Caller = RuleInst = caller;
    }
}