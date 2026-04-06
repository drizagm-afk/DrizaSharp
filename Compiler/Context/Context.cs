namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    //>>>> STATIC: CONTEXTS <<<<
    private static List<CompilationContext> _contexts = [];

    public static CompilationContext ContextAt(int id)
    => _contexts[id];
    public static int EnsureContext(string path)
    {
        var ctxId = _contexts.Count;
        _contexts.Add(new CompilationContext(ctxId));

        return ctxId;
    }

    //>>>> CONTEXT CREATION <<<<
    public readonly int Id;
    private CompilationContext(int id) => Id = id;
}