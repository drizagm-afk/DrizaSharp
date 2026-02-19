using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public class TokenPattern
{
    //VARIABLE REFERENCES
    internal string? _varName;

    public TokenPattern() { }
    public TokenPattern(string varName)
    { _varName = varName; }

    //PATTERN LIST
    private List<Func<ParserMatchContext, TokenSpan, int>> _patterns = [];

    public int PatternCount => _patterns.Count;
    public int CurPatternId => _patternId;
    private int _patternId;
    public void NewPattern(Func<ParserMatchContext, TokenSpan, int> pattern)
    => _patterns.Add(pattern);
    public int EvalPattern(int patternId, ParserMatchContext ctx, TokenSpan span)
    => _patterns[patternId].Invoke(ctx, span);

    //MATCH ENTRY
    internal int Matches(ParserMatchContext ctx, TokenSpan span)
    {
        //MATCH LOOP
        int i = 0;

        _patternId = 0;
        while (_patternId < _patterns.Count)
        {
            //VERIFYING TOKEN
            if (!ctx.HasTokenAtSpan(span, i)) return 0;

            //VERIFYING CHECK
            var _pattern = _patterns[_patternId];
            int res = _pattern.Invoke(ctx, span.Skip(i));
            if (res == 0) return 0;

            i += Math.Max(res, 0);

            //COUNTER
            _patternId++;
        }
        return i;
    }

    //DEFAULT PATTERNS
    public TokenPattern Token(byte type, string? val = null, string? captureTag = null)
    {
        NewPattern((ctx, span) =>
        {
            //MATCH
            var token = ctx.TokenAtSpan(span, 0);
            if (token.Type != type)
                return 0;
            if (val is not null && ctx.Stringify(token.Id) != val)
                return 0;

            //VAR
            if (captureTag is not null)
                ctx.StoreVar(captureTag, span.With(length: 1));

            return 1;
        });
        return this;
    }

    public TokenPattern Rule<R>(string? captureTag = null) where R : ParserRule
    {
        NewPattern((ctx, span) =>
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
    public TokenPattern RuleClass<C>(string? captureTag = null) where C : ParserRuleClass
    {
        NewPattern((ctx, span) =>
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