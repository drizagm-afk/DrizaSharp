using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public static class Binding
{
    //PHASES
    public static void SetPhases(params ParserPhase[] phases)
    => ParserManager._phases = phases;

    //REALMS
    public static RealmId AddRealm(int phaseCode)
    => new(phaseCode, ParserManager._phases[phaseCode].realmCount++);

    //RULES
    public static R BindRule<R>() where R : Rule, new()
    {
        var rule = new R { Id = new(false, ParserManager._rules.Count) };
        ParserManager._rules.Add(rule);
        ParserManager._rulesByType[typeof(R)] = rule.Id;

        return rule;
    }
    public static R BindRule<P, R>() where P : RuleClass where R : Rule, new()
    {
        var rule = BindRule<R>();
        rule.Parent = ParserManager.GetRuleClass<P>();

        return rule;
    }

    public static C BindRuleClass<C>() where C : RuleClass, new()
    {
        var rule = new C { Id = new(true, ParserManager._ruleClasses.Count) };
        ParserManager._ruleClasses.Add(rule);
        ParserManager._ruleClassesByType[typeof(C)] = rule.Id;

        return rule;
    }
    public static C BindRuleClass<P, C>() where P : RuleClass where C : RuleClass, new()
    {
        var rule = BindRuleClass<C>();
        rule.Parent = ParserManager.GetRuleClass<P>();

        return rule;
    }
}