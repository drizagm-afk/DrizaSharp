using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Default.Lexer;

public static class TokenType
{
    public const byte NEWLINE = Token.NEWLINE;
    public static byte Operator { get; internal set; }
    public static byte Keyword { get; internal set; }

    //OPENERS
    public static byte OpParen { get; internal set; }
    public static byte OpBrack { get; internal set; }
    public static byte OpBrace { get; internal set; }

    //CLOSERS
    public static byte ClParen { get; internal set; }
    public static byte ClBrack { get; internal set; }
    public static byte ClBrace { get; internal set; }

    //PREFIXES
    public static byte AtsignPrefix { get; internal set; }
    public static byte HashPrefix { get; internal set; }
    public static byte DollarPrefix { get; internal set; }

    //DATA
    public static byte Bool { get; internal set; }
    public static byte Number { get; internal set; }
    public static byte String { get; internal set; }
}