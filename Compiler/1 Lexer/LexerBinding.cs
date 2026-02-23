namespace DrzSharp.Compiler.Lexer;

public static class Binding
{
    public static byte AddTokenType()
    => LexerManager.typeCount++;

    public static void BindRule(Rule rule)
    => LexerManager._rules.Add(rule);
}