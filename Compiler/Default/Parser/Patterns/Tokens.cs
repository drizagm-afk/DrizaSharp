using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Patterns;

public static class Tokens
{
    //BASE
    public static Pattern nl(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.NEWLINE, null, captureTag);
    public static Pattern oper(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.Operator, val, captureTag);
    public static Pattern kw(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.Keyword, val, captureTag);

    //OPENERS & CLOSERS
    public static Pattern oparen(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.OpParen, null, captureTag);
    public static Pattern obrack(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.OpBrack, null, captureTag);
    public static Pattern obrace(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.OpBrace, null, captureTag);

    public static Pattern cparen(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.ClParen, null, captureTag);
    public static Pattern cbrack(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.ClBrack, null, captureTag);
    public static Pattern cbrace(this Pattern patt, int? captureTag = null)
    => patt.Token(TokenType.ClBrace, null, captureTag);

    //PREFIXES
    public static Pattern atsignPx(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.AtsignPrefix, val, captureTag);
    public static Pattern hashPx(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.HashPrefix, val, captureTag);
    public static Pattern dollarPx(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.DollarPrefix, val, captureTag);

    //VALUES
    public static Pattern boolLit(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.BoolLit, val, captureTag);
    public static Pattern numberLit(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.NumberLit, val, captureTag);
    public static Pattern stringLit(this Pattern patt, string? val = null, int? captureTag = null)
    => patt.Token(TokenType.StringLit, val, captureTag);
}