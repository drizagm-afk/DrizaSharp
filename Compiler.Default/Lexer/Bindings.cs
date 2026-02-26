using DrzSharp.Compiler.Lexer;

namespace DrzSharp.Compiler.Default.Lexer;

public static class Bindings
{
    public static void Bind()
    {
        BindTypes();
        BindRules();
    }

    //TOKEN TYPES
    private static void BindTypes()
    {
        TokenType.Operator = Binding.AddTokenType("Operator");
        TokenType.Keyword = Binding.AddTokenType("Keyword");

        TokenType.OpParen = Binding.AddTokenType("Parentheses Opener");
        TokenType.OpBrack = Binding.AddTokenType("Brackets Opener");
        TokenType.OpBrace = Binding.AddTokenType("Braces Opener");

        TokenType.ClParen = Binding.AddTokenType("Parentheses Closer");
        TokenType.ClBrack = Binding.AddTokenType("Brackets Closer");
        TokenType.ClBrace = Binding.AddTokenType("Braces Closer");

        TokenType.AtsignPrefix = Binding.AddTokenType("Atsign Prefix");
        TokenType.HashPrefix = Binding.AddTokenType("Hash Prefix");
        TokenType.DollarPrefix = Binding.AddTokenType("Dollar Prefix");

        TokenType.Bool = Binding.AddTokenType("Bool");
        TokenType.Number = Binding.AddTokenType("Number");
        TokenType.String = Binding.AddTokenType("String");
    }

    //RULES
    private static void BindRules()
    {
        Binding.BindRule(DefRules.OperatorRule);
        Binding.BindRule(DefRules.KeywordRule);

        Binding.BindRule(DefRules.OpenerRule);
        Binding.BindRule(DefRules.CloserRule);

        Binding.BindRule(DefRules.PrefixRule);

        Binding.BindRule(DefRules.BoolRule);
        Binding.BindRule(DefRules.NumberRule);
        Binding.BindRule(DefRules.StringRule);

        Binding.BindRule(DefRules.StringKeywordRule);
    }
}