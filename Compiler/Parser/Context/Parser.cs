using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public interface Context
{
    public int PhaseCode { get; }

    public R GetRule<R>() where R : Rule;
    public RuleId GetRuleId<R>() where R : Rule;
    public R GetRule<R>(RuleId id) where R : Rule;
    public Rule GetRule(RuleId id);

    public C GetRuleClass<C>() where C : RuleClass;
    public RuleId GetRuleClassId<C>() where C : RuleClass;
    public C GetRuleClass<C>(RuleId id) where C : RuleClass;
    public RuleClass GetRuleClass(RuleId id);

    //TAST DEFAULT CONTEXT
    public TASTNode NodeAt(int nodeId);

    public void Children(Action<int> action);
    public void Children(int nodeId, Action<int> action);

    public Token? TryTokenAtSpan(TokenSpan span, int tokenOrder);
    public bool TryTokenAtSpan(TokenSpan span, int tokenOrder, out Token token);
    public bool HasTokenAtSpan(TokenSpan span, int tokenOrder);
    public Token TokenAtSpan(TokenSpan span, int tokenOrder);

    public ReadOnlySpan<char> Stringify(int tokenId);
}
public partial class ParserProcess : Context
{
    //**PARSING PROCESS**
    public int PhaseCode => _phaseCode;

    public R GetRule<R>() where R : Rule
    => ParserManager.GetRule<R>();
    public RuleId GetRuleId<R>() where R : Rule
    => ParserManager.GetRuleId<R>();
    public R GetRule<R>(RuleId id) where R : Rule
    => ParserManager.GetRule<R>(id);
    public Rule GetRule(RuleId id)
    => ParserManager.GetRule(id);

    public C GetRuleClass<C>() where C : RuleClass
    => ParserManager.GetRuleClass<C>();
    public C GetRuleClass<C>(RuleId id) where C : RuleClass
    => ParserManager.GetRuleClass<C>(id);
    public RuleClass GetRuleClass(RuleId id)
    => ParserManager.GetRuleClass(id);
    public RuleId GetRuleClassId<C>() where C : RuleClass
    => ParserManager.GetRuleClassId<C>();

    //**TAST**
    //CHILDREN
    public void Children(Action<int> action) => TAST.Children(action);

    public void Children(int nodeId, Action<int> action) => TAST.Children(nodeId, action);

    //NODES
    public TASTNode NodeAt(int nodeId) => TAST.NodeAt(nodeId);

    //TOKENS
    public Token? TryTokenAtSpan(TokenSpan span, int order)
    {
        if (!InBounds(span, order)) return null;

        return TAST.TryTokenAtNode(span.NodeId, span.Offset, span.Start + order);
    }
    public bool TryTokenAtSpan(TokenSpan span, int order, out Token token)
    {
        if (!InBounds(span, order))
        {
            token = default;
            return false;
        }

        return TAST.TryTokenAtNode(span.NodeId, span.Offset, span.Start + order, out token);
    }
    public bool HasTokenAtSpan(TokenSpan span, int order)
    => InBounds(span, order) && TAST.HasTokenAtNode(span.NodeId, span.Offset, span.Start + order);
    public Token TokenAtSpan(TokenSpan span, int tokenOrder)
    {
        if (!InBounds(span, tokenOrder))
            throw new Exception($"TOKEN ORDER OUT OF BOUNDS: LENGTH={span.Length}, ORDER={tokenOrder}");

        return TAST.TokenAtNode(span.NodeId, span.Offset, span.Start + tokenOrder);
    }
    private static bool InBounds(TokenSpan span, int order)
    => (0 <= order) && (span.Length < 0 || order < span.Length);

    public ReadOnlySpan<char> Stringify(int tokenId) => TAST.Stringify(tokenId);
}