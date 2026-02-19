namespace DrzSharp.Compiler.Lexer;

public static class LexerBinding
{
    public static void BindRule<T>() where T : LexerRule, new()
    => LexerManager._rules.Add(new T());
}