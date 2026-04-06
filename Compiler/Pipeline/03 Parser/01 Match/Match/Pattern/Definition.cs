using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class Pattern
{
    //PATTERN LIST
    private List<Func<int, MatchContext, TokenSpan, int>> _patts = [];

    private Pattern? _parent;
    private int _nextRelId;

    public void AddPattern(Func<int, MatchContext, TokenSpan, int> patt)
    => _patts.Add(patt);
    public void AddSubPattern(Pattern subPatt)
    {
        subPatt._parent = this;
        subPatt._nextRelId = _patts.Count;
    }
    public void AddSubPatterns(params Pattern[] subPatts)
    {
        foreach(var subPatt in subPatts)
            AddSubPattern(subPatt);
    }

    public bool TryEvalPattern(int pattId, MatchContext ctx, TokenSpan span, out int res)
    {
        if (pattId < _patts.Count)
        {
            res = _patts[pattId].Invoke(pattId, ctx, span);
            return true;
        }
        else if (_parent is not null)
            return _parent.TryEvalPattern(_nextRelId + pattId - _patts.Count, ctx, span, out res);

        res = 0;
        return false;
    }

    //MATCH ENTRY
    public int Matches(MatchContext ctx, TokenSpan span)
    {
        //MATCH LOOP
        int i = 0;
        int pattId = 0;
        while (pattId < _patts.Count)
        {
            //VERIFYING TOKEN
            var evalSpan = span.Skip(i);
            if (!ctx.HasTokenAtSpan(evalSpan)) return 0;

            //VERIFYING CHECK
            var _pattern = _patts[pattId];
            int res = _pattern.Invoke(pattId, ctx, evalSpan);
            if (res == 0) return 0;

            i += Math.Max(res, 0);

            //COUNTER
            pattId++;
        }
        return i;
    }
}