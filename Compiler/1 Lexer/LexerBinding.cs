namespace DrzSharp.Compiler.Lexer;

public static class Binding
{
    public static byte AddTokenType(string tokenName)
    {
        var id = (byte)LexerManager.TokenTypes.Count;
        LexerManager.TokenTypes.Add(tokenName);
        return id;
    }

    public static void BindRule(Rule rule)
    => LexerManager._rules.Add(rule);
}