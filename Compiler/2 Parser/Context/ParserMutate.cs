using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface MutateContext : Context
{
    void Rewrite(RewriteToken[] tokens, int[] children, params RuleId[] rules);
}

public partial class ParserProcess : MutateContext
{
    //REWRITE
    public void Rewrite(RewriteToken[] tokens, int[] fillNodes, params RuleId[] rules)
    {
        if (tokens.Length <= 0)
            throw new Exception("CANNOT REWRITE INTO AN EMPTY EXPRESSION");

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

        //BASE
        RuleInst!.IsRewritten = true;
        var slice = TAST.NodeSourceSlice(RuleInst.NodeId);

        //REWRITE
        var start = TAST.TokenCount;
        foreach (var token in _reTokens)
        {
            if (token.IsNull) TAST.NewToken(Token.NULL, slice.Start, slice.Length);
            else TAST.NewToken(token.Type, slice.Start, slice.Length, token.Content);
        }
        TAST.Rewrite(RuleInst.NodeId, new(start, TAST.TokenCount - start), _reFillNodes);

        //EVAL REWRITE
        Match(RuleInst.NodeId, _reRules);

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
}