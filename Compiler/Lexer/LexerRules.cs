namespace DrzSharp.Compiler.Lexer;

public interface LexerRule
{
    public void TryMatch(LexerContext ctx, ReadOnlySpan<char> content);
}