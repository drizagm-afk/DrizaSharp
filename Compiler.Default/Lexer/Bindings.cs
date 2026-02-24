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
        TokenType.Operator = Binding.AddTokenType();
        TokenType.Keyword = Binding.AddTokenType();

        TokenType.OpParen = Binding.AddTokenType();
        TokenType.OpBrack = Binding.AddTokenType();
        TokenType.OpBrace = Binding.AddTokenType();

        TokenType.ClParen = Binding.AddTokenType();
        TokenType.ClBrack = Binding.AddTokenType();
        TokenType.ClBrace = Binding.AddTokenType();

        TokenType.AtsignPrefix = Binding.AddTokenType();
        TokenType.HashPrefix = Binding.AddTokenType();
        TokenType.DollarPrefix = Binding.AddTokenType();

        TokenType.Bool = Binding.AddTokenType();
        TokenType.Number = Binding.AddTokenType();
        TokenType.String = Binding.AddTokenType();
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
    }
}