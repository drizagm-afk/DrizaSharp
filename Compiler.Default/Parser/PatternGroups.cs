using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class PatternGroups
{
    //GROUP
    public static TokenPattern Group(this TokenPattern inst, string? captureTag = null)
    {
        inst.NewPattern((ctx, span) =>
        {
            var id = inst.CurPatternId;
            var length = 0;
            //MATCH
            while (ctx.HasTokenAtSpan(span, 0))
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

    //CLOSED GROUP
    public static TokenPattern ClosedGroup(this TokenPattern inst, string? captureTag = null)
    {
        inst.NewPattern((ctx, span) =>
        {
            var openerStack = new Stack<byte>();
            var id = inst.CurPatternId;
            var length = 0;
            //MATCH
            while (ctx.TryTokenAtSpan(span, 0, out var token))
            {
                var type = token.Type;
                if (IsOpener(type)) openerStack.Push(type);
                else if (IsCloser(type))
                {
                    if (openerStack.Count <= 0) return 0;
                    if (!Matches(openerStack.Pop(), type)) return 0;
                }

                span = span.Skip();
                length++;

                //EVAL ADJACENT PATTERNS
                if (openerStack.Count <= 0)
                {
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
            }
            if (openerStack.Count > 0) return 0;
            if (captureTag is not null) ctx.StoreVar(captureTag, span.With(length: length));
            return length;
        });
        return inst;
    }
    private static bool IsOpener(byte type)
    => type is TokenType.OpParen or TokenType.OpBrack or TokenType.OpBrace;
    private static bool IsCloser(byte type)
    => type is TokenType.ClParen or TokenType.ClBrack or TokenType.ClBrace;
    private static bool Matches(byte openerType, byte closerType)
    => (openerType, closerType) switch
    {
        (TokenType.OpParen, TokenType.ClParen) => true,
        (TokenType.OpBrack, TokenType.ClBrack) => true,
        (TokenType.OpBrace, TokenType.ClBrace) => true,
        _ => false
    };
}