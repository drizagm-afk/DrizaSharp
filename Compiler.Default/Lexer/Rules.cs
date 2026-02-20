using DrzSharp.Compiler.Lexer;

namespace DrzSharp.Compiler.Default.Lexer;

public static class DefRules
{
    public static void OperatorRule(Context ctx, ReadOnlySpan<char> content)
    {
        var i = 0;
        while (i < content.Length)
        {
            var c = content[i];
            if (c == ';') break;
            if (c == '(' || c == '[' || c == '{') break;
            if (c == ')' || c == ']' || c == '}') break;
            if (char.IsSymbol(c) || char.IsPunctuation(c)) { i++; continue; }

            break;
        }

        if (i > 0) ctx.NewToken(TokenType.Operator, i);
    }
    public static void KeywordRule(Context ctx, ReadOnlySpan<char> content)
    {
        var i = 0;
        while (i < content.Length)
        {
            var c = content[i];
            if (char.IsLetter(c) || c == '_') { i++; continue; }
            else if (char.IsDigit(c) && i > 0) { i++; continue; }

            break;
        }

        if (i > 0) ctx.NewToken(TokenType.Keyword, i);
    }

    public static void OpenerRule(Context ctx, ReadOnlySpan<char> content)
    {
        switch (content[0])
        {
            case '(': ctx.NewToken(TokenType.OpParen, 1); break;
            case '[': ctx.NewToken(TokenType.OpBrack, 1); break;
            case '{': ctx.NewToken(TokenType.OpBrace, 1); break;
        }
    }
    public static void CloserRule(Context ctx, ReadOnlySpan<char> content)
    {
        switch (content[0])
        {
            case ')': ctx.NewToken(TokenType.ClParen, 1); break;
            case ']': ctx.NewToken(TokenType.ClBrack, 1); break;
            case '}': ctx.NewToken(TokenType.ClBrace, 1); break;
        }
    }

    public static void PrefixRule(Context ctx, ReadOnlySpan<char> content)
    {
        var prefix = content[0];
        byte prefixType = prefix switch
        {
            '@' => TokenType.AtsignPrefix,
            '#' => TokenType.HashPrefix,
            '$' => TokenType.DollarPrefix,
            _ => 0
        };

        //PREFIX EVAL
        if (prefixType == 0) return;

        //KEYWORD EVAL
        var i = 1;
        while (i < content.Length)
        {
            var c = content[i];
            if (char.IsLetter(c) || c == '_') { i++; continue; }
            else if (char.IsDigit(c) && i > 1) { i++; continue; }

            break;
        }

        if (i > 1) ctx.NewToken(prefixType, i);
    }

    //DATA TYPES
    public static void BoolRule(Context ctx, ReadOnlySpan<char> content)
    {
        if (content.StartsWith("true", StringComparison.Ordinal))
            ctx.NewToken(TokenType.Bool, 4);
        else if (content.StartsWith("false", StringComparison.Ordinal))
            ctx.NewToken(TokenType.Bool, 5);
    }
    public static void NumberRule(Context ctx, ReadOnlySpan<char> content)
    {
        var i = 0;
        while (i < content.Length)
        {
            var c = content[i];
            if (char.IsDigit(c)) { i++; continue; }

            break;
        }

        if (i > 0) ctx.NewToken(TokenType.Number, i);
    }
    public static void StringRule(Context ctx, ReadOnlySpan<char> content)
    {
        var prefix = content[0];
        if (prefix != '\"' && prefix != '\'') return;

        var i = 1;
        while (i < content.Length)
        {
            var c = content[i];
            i++;

            if (c == prefix && content[i - 1] != '\\')
            {
                ctx.NewToken(TokenType.String, i);
                return;
            }
        }
    }
}