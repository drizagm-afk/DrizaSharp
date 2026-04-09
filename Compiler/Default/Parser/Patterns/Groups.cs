using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Default.Lexer;

namespace DrzSharp.Compiler.Default.Patterns;

public static class Groups
{
    public static Pattern Body(this Pattern patt, int? captureTag = null)
    => patt.ClosedGroup(captureTag);
    public static Pattern CGroup(this Pattern patt, int? captureTag = null)
    => patt.ClosedGroup(captureTag);
    public static Pattern ClosedGroup(this Pattern patt, int? captureTag = null)
    {
        patt.AddPattern((id, ctx, span) =>
        {
            var evalSpan = span;
            var openerStack = new Stack<int>();
            var length = 0;

            //MATCH
            while (ctx.TryTokenAtSpan(evalSpan, out var token))
            {
                //EVAL ADJACENT PATTERNS
                if (openerStack.Count == 0)
                {
                    int res = -1;
                    int next = 0;
                    while (res < 0 && patt.TryEvalPattern(id + next + 1, ctx, evalSpan.Skip(next), out res))
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
                if (IsOpener(ctx, type)) openerStack.Push(type);
                else if (IsCloser(ctx, type))
                {
                    if (openerStack.Count <= 0) return 0;
                    if (!ClosureMatches(ctx, openerStack.Pop(), type)) return 0;
                }

                //MOVE FORWARD
                evalSpan = evalSpan.Skip();
                length++;
            }
            if (openerStack.Count > 0) return 0;
            if (length > 0 && captureTag is int tag) ctx.StoreVar(tag, span.With(length: length));
            return length;
        });
        return patt;
    }
    private static bool IsOpener(Context ctx, int type)
    => type == ctx.TokenType(TokenType.OpParen)
    || type == ctx.TokenType(TokenType.OpBrack)
    || type == ctx.TokenType(TokenType.OpBrace);
    private static bool IsCloser(Context ctx, int type)
    => type == ctx.TokenType(TokenType.ClParen)
    || type == ctx.TokenType(TokenType.ClBrack)
    || type == ctx.TokenType(TokenType.ClBrace);
    private static bool ClosureMatches(Context ctx, int op, int cl)
    {
        if (op == ctx.TokenType(TokenType.OpParen)
        && cl == ctx.TokenType(TokenType.ClParen))
            return true;
        if (op == ctx.TokenType(TokenType.OpBrack) 
        && cl == ctx.TokenType(TokenType.ClBrack))
            return true;
        if (op == ctx.TokenType(TokenType.OpBrace) 
        && cl == ctx.TokenType(TokenType.ClBrace))
            return true;
        return false;
    }
}