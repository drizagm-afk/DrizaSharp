using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lexer;

public interface Context
{
    //TOKEN RESEARCH
    public bool TryTokenAt(int tokenId, out Token token);
    public bool HasTokenAt(int tokenId);
    public Token TokenAt(int tokenId);

    //TOKEN CREATION
    public int NewToken(byte type, int length);
    public int NewToken(byte type, int start, int length);
    public int NewToken(byte type, int length, string rephrase);
    public int NewToken(byte type, int start, int length, string rephrase);

    //TOKEN LIST
    public int TokenCount { get; }
    public Token LastToken();

    //TOKEN EVALUATION
    public ReadOnlySpan<char> Stringify(int tokenId);
}

public partial class LexerProcess : Context
{
    public bool TryTokenAt(int tokenId, out Token token)
    => TAST.TryTokenAt(tokenId, out token);
    public bool HasTokenAt(int tokenId) => TAST.HasTokenAt(tokenId);
    public Token TokenAt(int tokenId) => TAST.TokenAt(tokenId);

    public int NewToken(byte type, int length) 
    => NewToken(type, iter, length);
    public int NewToken(byte type, int start, int length) 
    => TAST.NewToken(type, start, length);
    public int NewToken(byte type, int length, string rephrase)
    => NewToken(type, iter, length, rephrase);
    public int NewToken(byte type, int start, int length, string rephrase)
    {
        var token = NewToken(type, start, length);
        TAST.Rephrase(token, rephrase);
        return token;
    }

    public int TokenCount => TAST.TokenCount;
    public Token LastToken() => TokenAt(TAST.TokenCount - 1);

    public ReadOnlySpan<char> Stringify(int tokenId) => TAST.Stringify(tokenId);
}