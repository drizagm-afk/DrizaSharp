using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public abstract class RuleInstance
{
    internal RuleId RuleId;
    public int NodeId = -1;

    //MATCH
    public TokenSpan Span;
    public RuleInstance? Parent { get; internal set; }
    internal bool IsRewritten = false;

    internal void Build(BuildContext ctx) => OnBuild(ctx);
    private protected virtual void OnBuild(BuildContext ctx) { }
    internal void Mutate(MutateContext ctx) => OnMutate(ctx);
    private protected virtual void OnMutate(MutateContext ctx) { }

    //VALIDATE
    internal void Validate(ValidateContext ctx) => OnValidate(ctx);
    private protected virtual void OnValidate(ValidateContext ctx) { }

    //EMIT
    internal EmitId EmitId;
    internal void Emit(EmitContext ctx) => OnEmit(ctx);
    private protected virtual void OnEmit(EmitContext ctx) { }
}