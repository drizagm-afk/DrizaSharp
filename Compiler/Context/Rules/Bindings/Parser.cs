using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Rules;

public interface ParserBinding : ArtifactsBinding
{
    //>>>> MODEL <<<<
    public int AddRealm(string realmName);
    public int Realm(string realmName);

    //>>>> RULES <<<<
    public C BindRuleClass<C>(string realmName) where C : RuleClass, new()
    => BindRuleClass<C>(Realm(realmName));
    public C BindRuleClass<C>(int realmId) where C : RuleClass, new();
    public C BindRuleClass<C, P>() where C : RuleClass, new() where P : RuleClass;

    public R BindRule<R>(string realmName, bool isAbstract = false) where R : Rule, new()
    => BindRule<R>(Realm(realmName), isAbstract);
    public R BindRule<R>(int realmId, bool isAbstract = false) where R : Rule, new();
    public R BindRule<R, P>(bool isAbstract = false) where R : Rule, new() where P : RuleClass;

    //>>>> HOOKS <<<<
    public void HookRuleClass<H, C>() where H : Hook, new() where C : RuleClass;
    public void HookRule<H, R>() where H : Hook, new() where R : Rule;
    public void HookPhase<H>() where H : PhaseHook, new();
}

internal partial class RulesetBinding : ParserBinding
{
    //>>>> MODEL <<<<
    int ParserBinding.AddRealm(string realmName)
    {
        var realms = _ruleset._pRealms;
        int id = realms.Count;

        realms.Add(new(realmName));
        _ruleset._pRealmsByKey[new(realmName)] = id;

        return id;
    }
    int ParserBinding.Realm(string realmName)
    => _ruleset._pRealmsByKey[new(realmName)];

    //>>>> RULES <<<<
    //RULE CLASSES
    private void BindClassToClass(int ruleId, RuleId classId)
    {
        var dict = _localRuleset._pRulesByClass;

        if (!dict.TryGetValue(classId, out var list))
            list = dict[classId] = [];

        list.Add((true, ruleId));
    }

    C ParserBinding.BindRuleClass<C>(int realmId)
    {
        var classes = _ruleset._pClasses;

        var id = classes.Count;
        var clazz = new C()
        {
            Id = NewClassRuleId(id),
            Name = typeof(C).Name,
            RealmId = realmId
        };

        classes.Add(clazz);
        _ruleset._pClassesByType[typeof(C)] = clazz;

        return clazz;
    }
    C ParserBinding.BindRuleClass<C, P>()
    {
        var parent = _ruleset._pClassesByType[typeof(P)];
        var clazz = ((ParserBinding)this).BindRuleClass<C>(parent.RealmId);

        clazz.Parent = parent;
        BindClassToClass(clazz.Id.LocalId, parent.Id);

        return clazz;
    }

    //RULES
    private void BindRuleToClass(int ruleId, RuleId classId)
    {
        var dict = _localRuleset._pRulesByClass;

        if (!dict.TryGetValue(classId, out var list))
            list = dict[classId] = [];

        list.Add((false, ruleId));
    }
    private void BindRuleToRealm(int ruleId, int realmId)
    {
        var dict = _localRuleset._pRulesByRealm;

        if (!dict.TryGetValue(realmId, out var list))
            list = dict[realmId] = [];

        list.Add(ruleId);
    }

    R ParserBinding.BindRule<R>(int realmId, bool isAbstract)
    {
        var rules = _localRuleset._pRules;

        var id = rules.Count;
        var rule = new R()
        {
            Id = NewRuleId(id),
            Name = typeof(R).Name,
            RealmId = realmId,
            IsAbstract = isAbstract
        };

        rules.Add(rule);
        BindRuleToRealm(id, realmId);

        _ruleset._pRulesByType[typeof(R)] = rule;

        return rule;
    }
    R ParserBinding.BindRule<R, P>(bool isAbstract)
    {
        var parent = _ruleset._pClassesByType[typeof(P)];
        var rule = ((ParserBinding)this).BindRule<R>(parent.RealmId, isAbstract);

        rule.Parent = parent;
        BindRuleToClass(rule.Id.LocalId, parent.Id);

        return rule;
    }

    //>>>> HOOKS <<<<
    private void BindHook<H>(RuleId ruleId) where H : Hook, new()
    {
        var hooks = _localRuleset._pHooks;

        if (!hooks.TryGetValue(ruleId, out var list))
            hooks[ruleId] = list = [];

        list.Add(new H());
    }

    void ParserBinding.HookRuleClass<H, C>()
    {
        var ruleId = _ruleset._pClassesByType[typeof(C)].Id;

        BindHook<H>(ruleId);
    }
    void ParserBinding.HookRule<H, R>()
    {
        var ruleId = _ruleset._pRulesByType[typeof(R)].Id;

        BindHook<H>(ruleId);
    }
    void ParserBinding.HookPhase<H>()
    {
        var hooks = _localRuleset._pPhaseHooks;

        hooks.Add(new H());
    }
}