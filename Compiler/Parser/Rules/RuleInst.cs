using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public abstract class ParserRuleInstance
{
    internal RuleId RuleId;
    public int NodeId = -1;
    public ParserRuleInstance? Parent { get; internal set; }

    public TokenSpan Span;

    internal void Build(ParserBuildContext ctx) => OnBuild(ctx);
    private protected virtual void OnBuild(ParserBuildContext ctx) { }
    internal void Mutate(ParserMutateContext ctx) => OnMutate(ctx);
    private protected virtual void OnMutate(ParserMutateContext ctx) { }
}