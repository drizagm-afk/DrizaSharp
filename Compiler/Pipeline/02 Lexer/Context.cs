using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lexer;

public interface Context
{
    //TOKEN RESEARCH
    public bool TryTokenAt(int tokenId, out Token token);
    public bool HasTokenAt(int tokenId);
    public Token TokenAt(int tokenId);
}
public partial class LexerProcess : Context
{
    public bool TryTokenAt(int tokenId, out Token token)
    => TAST.TryTokenAt(tokenId, out token);
    public bool HasTokenAt(int tokenId) => TAST.HasTokenAt(tokenId);
    public Token TokenAt(int tokenId) => TAST.TokenAt(tokenId);
}