using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class PatternExtra
{
    //TOKEN SHORTHANDS
    public static TokenPattern TKeyword (this TokenPattern inst, string? val = null, string? captureTag = null)
    => inst.Token(TokenType.Keyword, val, captureTag);

    //
    public static TokenPattern Sequence(this TokenPattern inst, Action<TokenPattern> mainPattern, Action<TokenPattern> linkPattern)
    => inst;

    public static TokenPattern Or(this TokenPattern inst, params Action<TokenPattern>[] subPatterns)
    => inst;

    public static TokenPattern Optional(this TokenPattern inst, Action<TokenPattern> subPattern)
    => inst;

    public static TokenPattern Repeat(this TokenPattern inst, Action<TokenPattern> subPattern, int min = 1, int? max = null)
    => inst;
}