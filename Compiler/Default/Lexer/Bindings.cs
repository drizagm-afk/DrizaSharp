using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules;

namespace DrzSharp.Compiler.Default.Lexer;

public static class Bindings
{
    public static void Bind(LexerBinding ctx)
    {
        TokenType.Bind(ctx);
        BindRules(ctx);
    }
    private static void BindRules(LexerBinding ctx)
    {
        ctx.BindRule(Ruleset.OperatorRule);
        ctx.BindRule(Ruleset.KeywordRule);

        ctx.BindRule(Ruleset.OpenerRule);
        ctx.BindRule(Ruleset.CloserRule);

        ctx.BindRule(Ruleset.PrefixRule);

        ctx.BindRule(Ruleset.BoolRule);
        ctx.BindRule(Ruleset.NumberRule);
        ctx.BindRule(Ruleset.StringRule);

        ctx.BindRule(Ruleset.StringKeywordRule);
        ctx.BindRule(Ruleset.EmojiKeywordRule);
    }
}
public static class TokenType
{
    internal static void Bind(LexerBinding ctx)
    {
        ctx.AddTokenType(Operator);
        ctx.AddTokenType(Keyword);

        ctx.AddTokenType(OpParen, false);
        ctx.AddTokenType(OpBrack, false);
        ctx.AddTokenType(OpBrace, false);

        ctx.AddTokenType(ClParen, false);
        ctx.AddTokenType(ClBrack, false);
        ctx.AddTokenType(ClBrace, false);

        ctx.AddTokenType(AtsignPrefix);
        ctx.AddTokenType(HashPrefix);
        ctx.AddTokenType(DollarPrefix);

        ctx.AddTokenType(BoolLit);
        ctx.AddTokenType(NumberLit);
        ctx.AddTokenType(StringLit);
    }

    //BASE
    public const string NEWLINE = Tokens.NEWLINE;
    public const string Operator = "Operator";
    public const string Keyword = "Keyword";

    //OPENERS & CLOSERS
    public const string OpParen = "Parentheses Opener";
    public const string OpBrack = "Brackets Opener";
    public const string OpBrace = "Braces Opener";

    public const string ClParen = "Parentheses Closer";
    public const string ClBrack = "Brackets Closer";
    public const string ClBrace = "Braces Closer";

    //PREFIXES
    public const string AtsignPrefix = "Atsign Prefix";
    public const string HashPrefix = "Hash Prefix";
    public const string DollarPrefix = "Dollar Prefix";

    //LITERALS
    public const string BoolLit = "Bool Literal";
    public const string NumberLit = "Number Literal";
    public const string StringLit = "String Literal";
}