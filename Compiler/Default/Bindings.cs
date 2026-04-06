using DrzSharp.Compiler.Rules;

namespace DrzSharp.Compiler.Default;

public static class Bindings
{
    public static void Bind(BindingContext ctx)
    {
        Lexer.Bindings.Bind(ctx.Lexer);
        Parser.Bindings.Bind(ctx.Parser);
    }
}