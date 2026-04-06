using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lexer;

public interface LexContext : Context
{
    //TOKEN CREATION
    public int NewToken(GlobalId type, int length);
    public int NewToken(GlobalId type, int start, int length);
    public int NewToken(GlobalId type, int length, string rephrase);
    public int NewToken(GlobalId type, int start, int length, string rephrase);

    //TOKEN LIST
    public int TokenCount { get; }
    public Token LastToken();

    //TOKEN EVALUATION
    public ReadOnlySpan<char> GetTextSpan(int tokenId);
    public string GetText(int tokenId);
}

public partial class LexerProcess : LexContext
{
    public int NewToken(GlobalId type, int length) 
    => NewToken(type, iter, length);
    public int NewToken(GlobalId type, int start, int length) 
    => TAST.NewToken(type, start, length);
    public int NewToken(GlobalId type, int length, string rephrase)
    => NewToken(type, iter, length, rephrase);
    public int NewToken(GlobalId type, int start, int length, string rephrase)
    {
        var token = NewToken(type, start, length);
        TAST.Rephrase(token, rephrase);
        return token;
    }

    public int TokenCount => TAST.TokenCount;
    public Token LastToken() => TokenAt(TAST.TokenCount - 1);

    public ReadOnlySpan<char> GetTextSpan(int tokenId) => TAST.GetTextSpan(tokenId);
    public string GetText(int tokenId) => TAST.GetText(tokenId);
}