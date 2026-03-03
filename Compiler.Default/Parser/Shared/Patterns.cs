using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Default.Parser;

public static class ShortPatterns
{
    //TOKEN SHORTCUTS
    public static TokenPattern nl(this TokenPattern inst, int? captureTag = null)
    => inst.newline(captureTag);
    public static TokenPattern kw(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.keyword(val, captureTag);

    //REGEX SHORTCUTS
    public static TokenPattern Opt(this TokenPattern inst, Action<TokenPattern> optPattern)
    => inst.Optional(optPattern);

    //COMPOUND SHORTCUTS
    public static TokenPattern OptNl(this TokenPattern inst, int? captureTag = null)
    => inst.Opt(t => t.newline(captureTag));

    //TABLES
    public static TokenPattern KeywordTable(this TokenPattern inst, params string[] vals)
    => inst.KeywordTable(0, vals);
    public static TokenPattern KeywordTable(this TokenPattern inst, int start, params string[] vals)
    {
        inst.AddPattern((_, ctx, span) =>
        {
            //MATCH
            var token = ctx.TokenAtSpan(span);
            if (token.Type != TokenType.Keyword)
                return 0;

            for (int i = 0; i < vals.Length; i++)
            {
                var val = vals[i];
                var txt = ctx.GetTextSpan(token.Id);

                if (txt.Length != val.Length)
                    continue;
                if (!txt.StartsWith(val))
                    continue;

                ctx.StoreVar(start + i, span.With(length: 1));
                return 1;
            }
            return 0;
        });

        return inst;
    }

    public static bool HasVarInRange(this MatchView view, Range range, out int tag)
    {
        for (int i = range.Start.Value; i < range.End.Value; i++)
        {
            if (!view.HasVar(i))
                continue;
            
            tag = i;
            return true;
        }
        tag = -1;
        return false;
    }
}

public static class TokenPatterns
{
    //BASE
    public static TokenPattern newline(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.NEWLINE, null, captureTag);
    public static TokenPattern oper(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.Operator, val, captureTag);
    public static TokenPattern keyword(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.Keyword, val, captureTag);

    //OPENERS
    public static TokenPattern oparen(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.OpParen, null, captureTag);
    public static TokenPattern obrack(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.OpBrack, null, captureTag);
    public static TokenPattern obrace(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.OpBrace, null, captureTag);

    //CLOSERS
    public static TokenPattern cparen(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.ClParen, null, captureTag);
    public static TokenPattern cbrack(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.ClBrack, null, captureTag);
    public static TokenPattern cbrace(this TokenPattern inst, int? captureTag = null)
    => inst.Token(TokenType.ClBrace, null, captureTag);

    //PREFIXES
    public static TokenPattern atsignpx(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.AtsignPrefix, val, captureTag);
    public static TokenPattern hashpx(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.HashPrefix, val, captureTag);
    public static TokenPattern dollarpx(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.DollarPrefix, val, captureTag);

    //VALUES
    public static TokenPattern boolVal(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.Bool, val, captureTag);
    public static TokenPattern numberVal(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.Number, val, captureTag);
    public static TokenPattern stringVal(this TokenPattern inst, string? val = null, int? captureTag = null)
    => inst.Token(TokenType.String, val, captureTag);
}

public static class GroupPatterns
{
    public static TokenPattern CGroup(this TokenPattern inst, int? captureTag = null)
    => inst.ClosedGroup(captureTag);
    public static TokenPattern ClosedGroup(this TokenPattern inst, int? captureTag = null)
    {
        inst.AddPattern((id, ctx, span) =>
        {
            var evalSpan = span;
            var openerStack = new Stack<byte>();
            var length = 0;

            //MATCH
            while (ctx.TryTokenAtSpan(evalSpan, out var token))
            {
                //EVAL ADJACENT PATTERNS
                if (openerStack.Count == 0)
                {
                    int res = -1;
                    int next = 0;
                    while (res < 0 && inst.TryEvalPattern(id + next + 1, ctx, evalSpan.Skip(next), out res))
                        next++;
                    if (res > 0)
                    {
                        length += next - 1;
                        if (length == 0) length = -1;

                        break;
                    }
                }

                //EVAL CLOSURE
                var type = token.Type;
                if (IsOpener(type)) openerStack.Push(type);
                else if (IsCloser(type))
                {
                    if (openerStack.Count <= 0) return 0;
                    if (!ClosureMatches(openerStack.Pop(), type)) return 0;
                }

                //MOVE FORWARD
                evalSpan = evalSpan.Skip();
                length++;
            }
            if (openerStack.Count > 0) return 0;
            if (length > 0 && captureTag is int tag) ctx.StoreVar(tag, span.With(length: length));
            return length;
        });
        return inst;
    }
    private static bool IsOpener(byte type)
    => type == TokenType.OpParen || type == TokenType.OpBrack || type == TokenType.OpBrace;
    private static bool IsCloser(byte type)
    => type == TokenType.ClParen || type == TokenType.ClBrack || type == TokenType.ClBrace;
    private static bool ClosureMatches(byte op, byte cl)
    {
        if (op == TokenType.OpParen && cl == TokenType.ClParen) return true;
        if (op == TokenType.OpBrack && cl == TokenType.ClBrack) return true;
        if (op == TokenType.OpBrace && cl == TokenType.ClBrace) return true;
        return false;
    }
}