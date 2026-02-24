using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public abstract class RuleInstance
{
    internal RuleId RuleId;
    public int NodeId = -1;
    public RuleInstance? Parent { get; internal set; }

    public TokenSpan Span;
    internal bool IsRewritten = false;

    internal void Build(BuildContext ctx) => OnBuild(ctx);
    private protected virtual void OnBuild(BuildContext ctx) { }
    internal void Mutate(MutateContext ctx) => OnMutate(ctx);
    private protected virtual void OnMutate(MutateContext ctx) { }

    internal void Validate(ValidateContext ctx) => OnValidate(ctx);
    private protected virtual void OnValidate(ValidateContext ctx) { }
    /*
    internal void Emit(EmitContext ctx) => OnEmit(ctx);
    private protected virtual void OnEmit(EmitContext ctx) { }  
    */
}