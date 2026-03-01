using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface MutateContext : Context
{
    public void SetScoped();
    public void Rewrite(RewriteToken[] tokens, int[] children, params int[] rules);
}

public partial class ParserProcess : MutateContext
{
    //UPDATE
    public void SetScoped()
    => TAST.UpdateInfo(RuleInst!.NodeId, isScoped: true);

    //REWRITE
    public void Rewrite(RewriteToken[] tokens, int[] fillNodes, params int[] rules)
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
    private int[] _reRules = null!;
    private void ApplyRewrite()
    {
        if (!_reWrite) return;

        //BASE
        var nodeId = RuleInst!.NodeId;

        TAST.UpdateInfo(nodeId, isRewritten: true);
        var slice = TAST.SourceSlice(nodeId);

        //REWRITE
        var start = TAST.TokenCount;
        foreach (var token in _reTokens)
        {
            if (token.IsNull) TAST.NewToken(Token.NULL, slice.Start, slice.Length);
            else TAST.NewToken(token.Type, slice.Start, slice.Length, token.Content);
        }
        TAST.Rewrite(nodeId, new(start, TAST.TokenCount - start), _reFillNodes);

        //EVAL REWRITE
        Match(nodeId, _reRules);

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

    public RewriteToken(byte type) : this(type, "") {}
    public bool IsNull => Type == Token.NULL;
}