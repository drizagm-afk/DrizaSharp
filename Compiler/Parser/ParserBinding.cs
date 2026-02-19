using DrzSharp.Compiler.Default.Parser;

namespace DrzSharp.Compiler.Parser;

public static class ParserBinding
{
    public static R BindRule<R>() where R : ParserRule, new()
    {
        var rule = new R { Id = new(false, ParserManager._rules.Count) };
        ParserManager._rules.Add(rule);
        ParserManager._rulesByType[typeof(R)] = rule.Id;

        return rule;
    }
    public static R BindRule<P, R>() where P : ParserRuleClass where R : ParserRule, new()
    {
        var rule = BindRule<R>();
        rule.Parent = ParserManager.GetRuleClass<P>();

        return rule;
    }

    public static C BindRuleClass<C>() where C : ParserRuleClass, new()
    {
        var rule = new C { Id = new(true, ParserManager._ruleClasses.Count) };
        ParserManager._ruleClasses.Add(rule);
        ParserManager._ruleClassesByType[typeof(C)] = rule.Id;

        return rule;
    }
    public static C BindRuleClass<P, C>() where P : ParserRuleClass where C : ParserRuleClass, new()
    {
        var rule = BindRuleClass<C>();
        rule.Parent = ParserManager.GetRuleClass<P>();

        return rule;
    }
}