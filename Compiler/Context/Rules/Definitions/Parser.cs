using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Rules.Parser;

//>>>> RULES <<<<
public abstract class RuleBase
{
    public RuleId Id { get; internal set; }
    public string Name { get; internal set; } = "";

    public RuleClass? Parent { get; internal set; }
    public int RealmId { get; internal set; }

    //COMMON
    internal abstract void Instantiate(MatchView view, RuleInstance inst);
}

public abstract class Rule : RuleBase
{
    public bool IsAbstract { get; internal set; }
    internal abstract RuleInstance NewInstance();

    //MATCH
    internal Pattern Pattern = new();
    protected void SetPattern(Action<Pattern> pattern) => pattern(Pattern);
}
public abstract class Rule<T> : Rule where T : RuleInstance, new()
{
    internal sealed override RuleInstance NewInstance() => new T() { RuleId = Id };

    //INSTANTIATION
    internal sealed override void Instantiate(MatchView view, RuleInstance inst)
    => OnInstantiate(view, (T)inst);
    protected virtual void OnInstantiate(MatchView view, T inst) { }
}

public abstract class RuleClass : RuleBase;
public abstract class RuleClass<T> : RuleClass where T : RuleInstance, new()
{
    internal sealed override void Instantiate(MatchView view, RuleInstance inst)
    => OnInstantiate(view, (T)inst);
    protected virtual void OnInstantiate(MatchView view, T inst) { }
}

public abstract class RuleInstance
{
    internal RuleId RuleId;
    public int NodeId { get; internal set; } = -1;

    //MATCH
    public TokenSpan Span { get; internal set; }
    public RuleInstance? Caller { get; internal set; }

    internal void Nest(NestContext ctx) => OnNest(ctx);
    protected virtual void OnNest(NestContext ctx) { }

    internal void Build(BuildContext ctx) => OnBuild(ctx);
    protected virtual void OnBuild(BuildContext ctx) { }
    internal void BuildMutate(MutateContext ctx) => OnBuildMutate(ctx);
    protected virtual void OnBuildMutate(MutateContext ctx) { }

    //MUTATE
    public bool IsRewritten { get; internal set; }
    internal void Rewrite(Context ctx) => OnRewrite(ctx);
    protected virtual void OnRewrite(Context ctx)
    {
        BypassEmit = true;
    }

    internal void Append(Context ctx, int appendId) => OnAppend(ctx, appendId);
    protected virtual void OnAppend(Context ctx, int appendId)
    {
        throw new NotSupportedException($"The Rule {RuleId} doesn't support Appending");
    }

    //BIND
    internal void Bind(BindContext ctx) => OnBind(ctx);
    protected virtual void OnBind(BindContext ctx) { }
    internal void BindMutate(SemanticMutateContext ctx) => OnBindMutate(ctx);
    protected virtual void OnBindMutate(SemanticMutateContext ctx) { }

    //BIND DATA
    internal void BindData(BindDataContext ctx) => OnBindData(ctx);
    protected virtual void OnBindData(BindDataContext ctx) { }
    internal void BindDataMutate(SemanticMutateContext ctx) => OnBindDataMutate(ctx);
    protected virtual void OnBindDataMutate(SemanticMutateContext ctx) { }

    //VALIDATE
    public Validity Validity { get; internal set; }

    internal void Validate(ValidateContext ctx) => OnValidate(ctx);
    protected virtual void OnValidate(ValidateContext ctx) { }
    internal void ValidateMutate(SemanticMutateContext ctx) => OnValidateMutate(ctx);
    protected virtual void OnValidateMutate(SemanticMutateContext ctx) { }

    //EMIT
    public EmitTarget EmitTarget { get; internal set; }
    protected internal bool BypassEmit = false;

    internal void Emit(EmitContext ctx) => OnEmit(ctx);
    protected virtual void OnEmit(EmitContext ctx) { }
}
public enum Validity
{ None, Invalid, Valid }
public readonly struct EmitTarget(int nodeId, int instrId)
{
    public readonly int NodeId = nodeId;
    public readonly int InstrId = instrId;
}

//>>>> HOOKS <<<<
public abstract class Hook
{

}
public abstract class PhaseHook
{

}