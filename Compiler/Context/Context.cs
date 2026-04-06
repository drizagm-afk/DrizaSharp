namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    //>>>> STATIC: CONTEXTS <<<<
    private static List<CompilationContext> _contexts = [];

    public static CompilationContext ContextAt(int id)
    => _contexts[id];
    public static CompilationContext EnsureContext(string path)
    {
        var ctx = new CompilationContext(_contexts.Count);
        _contexts.Add(ctx);

        return ctx;
    }

    //>>>> CONTEXT CREATION <<<<
    public readonly int Id;
    private CompilationContext(int id) => Id = id;
}