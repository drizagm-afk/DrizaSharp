using System.Data;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public abstract class ParserRuleBase
{
    internal RuleId Id;
    internal ParserRuleClass? Parent;

    internal bool IsAbstract = false;
    private protected void SetAbstract() => IsAbstract = true;

    //PARSER
    internal protected virtual bool Evals(ParserContext ctx, in TASTArgs args) => true;

    internal TokenPattern[] Patterns = [];
    private protected void SetPatterns(params TokenPattern[] patterns) => Patterns = patterns;

    internal abstract void Instantiate(ParserMatchView view, ParserRuleInstance instance);
}

//RULE
public abstract class ParserRule : ParserRuleBase
{
    internal abstract ParserRuleInstance NewInstance();
}
public abstract class ParserRule<T> : ParserRule where T : ParserRuleInstance, new()
{
    //INSTANTIATION
    internal sealed override ParserRuleInstance NewInstance() => new T() { RuleId = Id };
    internal sealed override void Instantiate(ParserMatchView view, ParserRuleInstance instance)
    => OnInstantiate(view, (T)instance);
    private protected virtual void OnInstantiate(ParserMatchView view, T instance) { }
}

//RULE CLASS
public abstract class ParserRuleClass : ParserRuleBase
{
    internal readonly List<RuleId> SubRules = [];
}
public abstract class ParserRuleClass<T> : ParserRuleClass where T : ParserRuleInstance, new()
{
    internal sealed override void Instantiate(ParserMatchView view, ParserRuleInstance instance)
    => OnInstantiate(view, (T)instance);
    private protected virtual void OnInstantiate(ParserMatchView view, T instance) { }
}

public readonly struct RuleId(bool isClass, int id)
{
    public readonly bool IsClass = isClass;
    public readonly int Id = id;
    public bool Equals(RuleId other)
    => Id == other.Id && IsClass == other.IsClass;
}