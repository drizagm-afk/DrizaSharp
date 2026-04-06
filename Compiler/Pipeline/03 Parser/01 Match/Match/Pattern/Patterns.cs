using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class Pattern
{
    //DEFAULT PATTERNS
    public Pattern Token(string tokenName, string? val = null, int? captureTag = null)
    {
        AddPattern((_, ctx, span) =>
        {
            //MATCH
            var token = ctx.TokenAtSpan(span);
            if (token.Type != ctx.TokenType(tokenName))
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
    public Pattern Token(int type, string? val = null, int? captureTag = null)
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
    public Pattern Rule<R>(int? captureTag = null) where R : Rule
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.MatchRule<R>(span);
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
    public Pattern RuleClass<C>(int? captureTag = null) where C : RuleClass
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.MatchRuleClass<C>(span);
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
    public Pattern Realm(string realmName, int? captureTag = null)
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.MatchRealm(ctx.Realm(realmName), span);
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
    public Pattern Realm(int realm, int? captureTag = null)
    {
        AddPattern((_, ctx, span) =>
        {
            var hash = ctx.Hash;
            ctx.NewHash();

            var inst = ctx.MatchRealm(realm, span);
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
    public Pattern Or(params Action<Pattern>[] patterns)
    {
        var patts = new Pattern[patterns.Length];
        for (int i = 0; i < patts.Length; i++)
        {
            Pattern patt = new();
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

    public Pattern Optional(Action<Pattern> optPattern)
    {
        Pattern patt = new();
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

    public Pattern Repeat(Action<Pattern> repPattern, int min = 1, int max = -1)
    {
        Pattern patt = new();
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