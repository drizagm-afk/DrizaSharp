using DrzSharp.Compiler.Lexer;

namespace DrzSharp.Compiler.Rules.Lexer;

//>>>> RULES <<<<
public delegate void Rule(LexContext ctx, ReadOnlySpan<char> content);

//>>>> HOOKS <<<<
public class PhaseHook
{
    
}