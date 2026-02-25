using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public abstract class RuleBase
{
    internal int Id;
    internal RuleClass? Parent;

    internal bool IsAbstract = false;
    private protected void SetAbstract() => IsAbstract = true;

    //PARSER
    private RealmId RealmId;
    private protected void SetRealm(RealmId realmId) => RealmId = realmId;
    public virtual bool Evals(RealmId realmId) => realmId.Equals(RealmId);

    internal TokenPattern[] Patterns = [];
    private protected void SetPatterns(params TokenPattern[] patterns) => Patterns = patterns;

    internal abstract void Instantiate(MatchView view, RuleInstance instance);
}

//RULE
public abstract class Rule : RuleBase
{
    internal abstract RuleInstance NewInstance();
}
public abstract class Rule<T> : Rule where T : RuleInstance, new()
{
    //INSTANTIATION
    internal sealed override RuleInstance NewInstance() => new T() { RuleId = Id };
    internal sealed override void Instantiate(MatchView view, RuleInstance instance)
    => OnInstantiate(view, (T)instance);
    protected abstract void OnInstantiate(MatchView view, T instance);
}

//RULE CLASS
public abstract class RuleClass : RuleBase
{
    internal readonly List<int> SubRules = [];
}
public abstract class RuleClass<T> : RuleClass where T : RuleInstance, new()
{
    internal sealed override void Instantiate(MatchView view, RuleInstance instance)
    => OnInstantiate(view, (T)instance);
    protected abstract void OnInstantiate(MatchView view, T instance);
}