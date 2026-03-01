using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class TokenPatterns
{
    public static TokenPattern TNewline(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.NEWLINE, null, captureTag);
    public static TokenPattern TOperator(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.Operator, val, captureTag);
    public static TokenPattern TKeyword(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.Keyword, val, captureTag);

    public static TokenPattern TOpParen(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.OpParen, null, captureTag);
    public static TokenPattern TOpBrack(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.OpBrack, null, captureTag);
    public static TokenPattern TOpBrace(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.OpBrace, null, captureTag);

    public static TokenPattern TClParen(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.ClParen, null, captureTag);
    public static TokenPattern TClBrack(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.ClBrack, null, captureTag);
    public static TokenPattern TClBrace(this TokenPattern inst, string? captureTag = null)
    => inst.Token(TokenType.ClBrace, null, captureTag);

    public static TokenPattern TAtsignPrefix(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.AtsignPrefix, val, captureTag);
    public static TokenPattern THashPrefix(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.HashPrefix, val, captureTag);
    public static TokenPattern TDollarPrefix(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.DollarPrefix, val, captureTag);

    public static TokenPattern TBool(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.Bool, val, captureTag);
    public static TokenPattern TNumber(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.Number, val, captureTag);
    public static TokenPattern TString(this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.String, val, captureTag);
}

public static class GroupPatterns
{
    public static TokenPattern Group(this TokenPattern inst, string? captureTag = null)
    {
        inst.NewPattern((id, ctx, span) =>
        {
            var length = 0;
            //MATCH
            while (ctx.HasTokenAtSpan(span))
            {
                span = span.Skip();
                length++;

                //EVAL ADJACENT PATTERNS
                int res = -1;
                int next = 0;
                int nextId = id + 1;
                while (nextId < inst.PatternCount && res < 0)
                {
                    res = inst.EvalPattern(nextId, ctx, span.Skip(next));
                    next++;
                    nextId++;
                }
                if (res > 0)
                {
                    length += next + res;
                    break;
                }
            }
            if (captureTag is not null) ctx.StoreVar(captureTag, span.With(length: length));
            return length;
        });
        return inst;
    }
    public static TokenPattern ClosedGroup(this TokenPattern inst, string? captureTag = null)
    {
        inst.NewPattern((id, ctx, span) =>
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
                    while (id + next + 1 < inst.PatternCount && res < 0)
                    {
                        res = inst.EvalPattern(id + next + 1, ctx, evalSpan.Skip(next));
                        next++;
                    }
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
            if (length > 0 && captureTag is not null) ctx.StoreVar(captureTag, span.With(length: length));
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

public static class UtilPatterns
{
    public static TokenPattern Sequence(this TokenPattern inst, Action<TokenPattern> mainPattern, Action<TokenPattern> linkPattern)
    => inst;

    public static TokenPattern Or(this TokenPattern inst, params Action<TokenPattern>[] subPatterns)
    => inst;

    public static TokenPattern Optional(this TokenPattern inst, Action<TokenPattern> subPattern)
    => inst;

    public static TokenPattern Repeat(this TokenPattern inst, Action<TokenPattern> subPattern, int min = 1, int? max = null)
    => inst;
}