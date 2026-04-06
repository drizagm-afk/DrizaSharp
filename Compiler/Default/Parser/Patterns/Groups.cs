using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Default.Lexer;

namespace DrzSharp.Compiler.Default.Patterns;

public static class Groups
{
    public static Pattern CGroup(this Pattern patt, int? captureTag = null)
    => patt.ClosedGroup(captureTag);
    public static Pattern ClosedGroup(this Pattern patt, int? captureTag = null)
    {
        patt.AddPattern((id, ctx, span) =>
        {
            var evalSpan = span;
            var openerStack = new Stack<GlobalId>();
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
        return patt;
    }
    private static bool IsOpener(GlobalId type)
    => type == TokenType.OpParen || type == TokenType.OpBrack || type == TokenType.OpBrace;
    private static bool IsCloser(GlobalId type)
    => type == TokenType.ClParen || type == TokenType.ClBrack || type == TokenType.ClBrace;
    private static bool ClosureMatches(GlobalId op, GlobalId cl)
    {
        if (op == TokenType.OpParen && cl == TokenType.ClParen) return true;
        if (op == TokenType.OpBrack && cl == TokenType.ClBrack) return true;
        if (op == TokenType.OpBrace && cl == TokenType.ClBrace) return true;
        return false;
    }
}