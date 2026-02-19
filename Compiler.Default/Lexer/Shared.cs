namespace DrzSharp.Compiler.Default.Lexer;

public static class TokenType
{
    public const byte Newline = 0;
    public const byte Operator = 1;
    public const byte Keyword = 2;

    //OPENERS
    public const byte OpParen = 3;
    public const byte OpBrack = 4;
    public const byte OpBrace = 5;

    //CLOSERS
    public const byte ClParen = 6;
    public const byte ClBrack = 7;
    public const byte ClBrace = 8;

    //PREFIXES
    public const byte AtsignPrefix = 9;
    public const byte HashPrefix = 10;
    public const byte DollarPrefix = 11;

    //DATA
    public const byte Bool = 12;
    public const byte Number = 13;
    public const byte String = 14;
}