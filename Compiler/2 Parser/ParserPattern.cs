using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public class TokenPattern
{
    //VARIABLE REFERENCES
    internal string? _varName;

    public TokenPattern() { }
    public TokenPattern(string varName)
    { _varName = varName; }

    //PATTERN LIST
    private List<Func<int, MatchContext, TokenSpan, int>> _patterns = [];
    public int PatternCount => _patterns.Count;

    public void NewPattern(Func<int, MatchContext, TokenSpan, int> pattern)
    => _patterns.Add(pattern);
    public int EvalPattern(int patternId, MatchContext ctx, TokenSpan span)
    => _patterns[patternId].Invoke(patternId, ctx, span);

    //MATCH ENTRY
    internal int Matches(MatchContext ctx, TokenSpan span)
    {
        //MATCH LOOP
        int i = 0;
        int pattId = 0;
        while (pattId < _patterns.Count)
        {
            //VERIFYING TOKEN
            var evalSpan = span.Skip(i);
            if (!ctx.HasTokenAtSpan(evalSpan)) return 0;

            //VERIFYING CHECK
            var _pattern = _patterns[pattId];
            int res = _pattern.Invoke(pattId, ctx, evalSpan);
            if (res == 0) return 0;

            i += Math.Max(res, 0);

            //COUNTER
            pattId++;
        }
        return i;
    }

    //DEFAULT PATTERNS
    public TokenPattern Token(byte type, string? val = null, string? captureTag = null)
    {
        NewPattern((_, ctx, span) =>
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
            if (captureTag is not null)
                ctx.StoreVar(captureTag, span.With(length: 1));

            return 1;
        });
        return this;
    }

    public TokenPattern Rule<R>(string? captureTag = null) where R : Rule
    {
        NewPattern((_, ctx, span) =>
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
            if (captureTag != null)
                ctx.StoreRuleVar(captureTag, inst);

            ctx.LoadHash(hash);
            return inst.Span.Length;
        });
        return this;
    }
    public TokenPattern RuleClass<C>(string? captureTag = null) where C : RuleClass
    {
        NewPattern((_, ctx, span) =>
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
            if (captureTag != null)
                ctx.StoreRuleVar(captureTag, inst);

            ctx.LoadHash(hash);
            return inst.Span.Length;
        });
        return this;
    }
}