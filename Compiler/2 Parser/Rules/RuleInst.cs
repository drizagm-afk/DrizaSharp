using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public abstract class RuleInstance
{
    internal int RuleId;
    public int NodeId = -1;

    //MATCH
    public TokenSpan Span;
    public RuleInstance? Parent { get; internal set; }

    internal void Build(BuildContext ctx) => OnBuild(ctx);
    protected virtual void OnBuild(BuildContext ctx) { }
    internal void Mutate(MutateContext ctx) => OnMutate(ctx);
    protected virtual void OnMutate(MutateContext ctx) { }

    //VALIDATE
    internal void Validate(ValidateContext ctx) => OnValidate(ctx);
    protected virtual void OnValidate(ValidateContext ctx) { }

    //EMIT
    internal TASTEmit EmitId;
    internal void Emit(EmitContext ctx) => OnEmit(ctx);
    protected virtual void OnEmit(EmitContext ctx) { }
}