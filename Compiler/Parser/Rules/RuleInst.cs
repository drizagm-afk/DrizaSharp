using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public abstract class RuleInstance
{
    internal RuleId RuleId;
    public int NodeId = -1;
    public RuleInstance? Parent { get; internal set; }

    public TokenSpan Span;

    internal void Build(BuildContext ctx) => OnBuild(ctx);
    private protected virtual void OnBuild(BuildContext ctx) { }
    internal void Mutate(MutateContext ctx) => OnMutate(ctx);
    private protected virtual void OnMutate(MutateContext ctx) { }
}