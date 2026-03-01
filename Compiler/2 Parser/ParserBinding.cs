using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public static class Binding
{
    //REALMS
    public static int AddRealm(string realmName)
    {
        int id = ParserManager.Realms.Count;
        ParserManager.Realms.Add(realmName);
        return id;
    }

    //RULES
    public static void BindRule<R>() where R : Rule, new()
    {
        var ruleId = ParserManager._rules.Count;
        var id = ParserManager.AddRuleInfo<R>(ruleId, false);

        ParserManager._rules.Add(new R { Id = id });
    }
    public static void BindRule<P, R>() where P : RuleClass where R : Rule, new()
    {
        var ruleId = ParserManager._rules.Count;
        var id = ParserManager.AddRuleInfo<R>(ruleId, false);

        ParserManager._rules.Add(new R { Id = id, Parent = ParserManager.GetRuleClass<P>() });
    }

    public static void BindRuleClass<C>() where C : RuleClass, new()
    {
        var ruleId = ParserManager._ruleClasses.Count;
        var id = ParserManager.AddRuleInfo<C>(ruleId, true);

        ParserManager._ruleClasses.Add(new C { Id = id });
    }
    public static void BindRuleClass<P, C>() where P : RuleClass where C : RuleClass, new()
    {
        var ruleId = ParserManager._ruleClasses.Count;
        var id = ParserManager.AddRuleInfo<C>(ruleId, true);

        ParserManager._ruleClasses.Add(new C { Id = id, Parent = ParserManager.GetRuleClass<P>() });
    }
}