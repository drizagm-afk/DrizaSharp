namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    internal void BindRuleset(int assemblyId, int nspaceId)
    {
        TryBindRuleset(assemblyId, nspaceId);
    }

    //>>>> RULESET LOADING <<<<
    private void TryBindRuleset(int assemblyId, int nspaceId)
    {
        _ruleset.Binding.Initialize(assemblyId, nspaceId);
        try
        {
            Default.Bindings.Bind(_ruleset.Binding);
        }
        catch
        {
            throw;
        }
    }
}