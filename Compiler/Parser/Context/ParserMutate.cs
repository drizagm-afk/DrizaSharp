using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public interface ParserMutateContext : ParserContext, ParserPassContext
{
    void Update(SchemeTASTArgs args);
    void Rewrite(RewriteToken[] tokens, int[] children, params RuleId[] rules);
}

public partial class ParserProcess : ParserMutateContext
{
    //REWRITE
    public void Update(SchemeTASTArgs args)
    => TAST.Update(RuleInst!.NodeId, args);
    public void Rewrite(RewriteToken[] tokens, int[] fillNodes, params RuleId[] rules)
    {
        _reWrite = true;
        _reTokens = tokens;
        _reFillNodes = fillNodes;
        _reRules = rules;
    }

    private bool _reWrite = false;
    private RewriteToken[] _reTokens = null!;
    private int[] _reFillNodes = null!;
    private RuleId[] _reRules = null!;
    private void ApplyRewrite()
    {
        if (!_reWrite) return;

        //REWRITE
        var start = TAST.TokenCount;
        foreach (var token in _reTokens)
        {
            if (token.IsNull) TAST.NewNullToken();
            else TAST.NewToken(token.Type, token.Content);
        }
        TAST.Rewrite(RuleInst!.NodeId, new(start, TAST.TokenCount - start), _reFillNodes);

        //EVAL REWRITE
        EvalMatch(RuleInst.NodeId, _reRules);

        ResetRewrite();
    }
    private void ResetRewrite()
    {
        _reWrite = false;
        _reTokens = null!;
        _reFillNodes = null!;
        _reRules = null!;
    }
}

public readonly struct RewriteToken(byte type, string content)
{
    public readonly byte Type = type;
    public readonly string Content = content;

    public bool IsNull => Content is null;
    public static RewriteToken Null => new(0, null!);
}