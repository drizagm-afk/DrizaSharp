using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

public static class Bindings
{
    public static void Bind()
    {
        Virtual.Bind();
        Logic.Bind();
    }
}