using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public class TokenPattern
{
    //VARIABLE REFERENCES
    internal readonly int? _varTag;

    public TokenPattern() { }
    public TokenPattern(int varTag)
    { _varTag = varTag; }

    //PATTERN LIST
    private List<Func<int, MatchContext, TokenSpan, int>> _patts = [];

    private TokenPattern? _parent;
    private int _nextRelId;

    public void AddPattern(Func<int, MatchContext, TokenSpan, int> patt)
    => _patts.Add(patt);
    public void AddSubPattern(TokenPattern subPatt)
    {
        subPatt._parent = this;
        subPatt._nextRelId = _patts.Count;
    }
    public void AddSubPatterns(params TokenPattern[] subPatts)
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

    //DEFAULT PATTERNS
    public TokenPattern Token(byte type, string? val = null, int? captureTag = null)
    {
        AddPattern((_, ctx, span) =>
        {
            //MATCH
            var token = ctx.TokenAtSpan(span);
            if (token.Type != type)
                return 0;
            if (val is not null)
            {
                var txt = ctx.GetTextSpan(token.Id);
                if (txt.Length != val.Length)
                    return 0;
                if (!txt.StartsWith(val))
                    return 0;
            }

            //VAR
            if (captureTag is int tag)
                ctx.StoreVar(tag, span.With(length: 1));

            return 1;
        });
        return this;
    }

    public TokenPattern Rule<R>(int? captureTag = null) where R : Rule
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.EvalRule<R>(span);
            if (inst is null)
            {
                ctx.LoadHash(hash);
                return 0;
            }

            //RETURN
            ctx.LoadHash(hash);
            if (captureTag is int tag)
                ctx.StoreRuleVar(tag, inst);

            return inst.Span.Length;
        });
        return this;
    }
    public TokenPattern RuleClass<C>(int? captureTag = null) where C : RuleClass
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.EvalRuleClass<C>(span);
            if (inst is null)
            {
                ctx.LoadHash(hash);
                return 0;
            }

            //RETURN
            ctx.LoadHash(hash);
            if (captureTag is int tag)
                ctx.StoreRuleVar(tag, inst);

            return inst.Span.Length;
        });
        return this;
    }

    //REGEX PATTERNS
    public TokenPattern Or(params Action<TokenPattern>[] patterns)
    {
        var patts = new TokenPattern[patterns.Length];
        for (int i = 0; i < patts.Length; i++)
        {
            TokenPattern patt = new();
            patts[i] = patt;
            patterns[i](patt);
        }

        AddPattern((_, ctx, span) =>
        {
            foreach (var patt in patts)
            {
                var iterCommit = ctx.Commit();
                int len = patt.Matches(ctx, span);
                if (len <= 0)
                    ctx.Rollback(iterCommit);
                else
                    return len;
            }

            return 0;
        });
        AddSubPatterns(patts);

        return this;
    }

    public TokenPattern Optional(Action<TokenPattern> optPattern)
    {
        TokenPattern patt = new();
        optPattern(patt);

        AddPattern((_, ctx, span) =>
        {
            var commit = ctx.Commit();

            var len = patt.Matches(ctx, span);
            if (len <= 0)
            {
                ctx.Rollback(commit);
                return -1;
            }

            return len;
        });
        AddSubPattern(patt);

        return this;
    }

    public TokenPattern Repeat(Action<TokenPattern> repPattern, int min = 1, int max = -1)
    {
        TokenPattern patt = new();
        repPattern(patt);

        AddPattern((_, ctx, span) =>
        {
            var srtCommit = ctx.Commit();

            int total = 0;
            int count = 0;

            var evalSpan = span;

            while (max == -1 || count < max)
            {
                var iterCommit = ctx.Commit();
                int len = patt.Matches(ctx, evalSpan);

                if (len <= 0)
                {
                    ctx.Rollback(iterCommit);
                    break;
                }

                total += len;
                evalSpan = evalSpan.Skip(len);
                count++;
            }

            if (count < min)
            {
                ctx.Rollback(srtCommit);
                return 0;
            }

            return total > 0 ? total : -1;
        });
        AddSubPattern(patt);

        return this;
    }
}