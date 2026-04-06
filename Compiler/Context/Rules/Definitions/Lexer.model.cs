namespace DrzSharp.Compiler.Model;

//>>>> TOKENS <<<<
public static class Tokens
{
    public const string NULL = "\0";
    public const byte NULL_ID = 0;

    public const string NEWLINE = "Newline";
    public const byte NEWLINE_ID = 1;
}
public readonly struct TokenTypeData(string name, bool showValue, bool mustParse)
{
    public readonly string Name = name;
    public readonly bool ShowValue = showValue;
    public readonly bool MustParse = mustParse;
}
public readonly record struct TokenTypeKey(string Name);