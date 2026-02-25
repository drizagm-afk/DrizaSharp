namespace DrzSharp.Compiler.Default;

public static class Bindings
{
    public static void Bind()
    {
        Lexer.Bindings.Bind();
        Parser.Bindings.Bind();
        Lowerer.Bindings.Bind();
    }
}