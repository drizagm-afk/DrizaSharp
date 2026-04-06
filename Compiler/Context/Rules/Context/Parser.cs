using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Rules.Parser;

public static class RuleExt
{
    //>>>> REALMS <<<<
    public static RealmData RealmAt(this DzProject proj, int id)
    => proj.Ruleset()._pRealms[id];
    public static int RealmId(this DzProject proj, string realmName)
    => proj.Ruleset()._pRealmsByKey[new(realmName)];
    public static RealmData Realm(this DzProject proj, string realmName)
    => proj.RealmAt(proj.RealmId(realmName));

    //>>>> RULES <<<<
    public static R GetRule<R>(this DzProject proj) where R : Rule
    => (R)proj.Ruleset()._pRulesByType[typeof(R)];
    public static RuleId GetRuleId<R>(this DzProject proj) where R : Rule
    => proj.GetRule<R>().Id;

    public static Rule GetRule(this DzProject proj, RuleId id)
    => proj.Ruleset(id)._pRules[id.LocalId];
    public static R GetRule<R>(this DzProject proj, RuleId id) where R : Rule
    => (R)proj.GetRule(id);

    //RULE CLASSES
    public static C GetRuleClass<C>(this DzProject proj) where C : RuleClass
    => (C)proj.Ruleset()._pClassesByType[typeof(C)];
    public static RuleId GetRuleClassId<C>(this DzProject proj) where C : RuleClass
    => proj.GetRuleClass<C>().Id;

    public static RuleClass GetRuleClass(this DzProject proj, RuleId id)
    => proj.Ruleset()._pClasses[id.LocalId];
    public static C GetRuleClass<C>(this DzProject proj, RuleId id) where C : RuleClass
    => (C)proj.GetRuleClass(id);

    public static bool IsRuleClass(this RuleId id)
    => id.NspaceId < 0;

    //MATCHING
    public static IEnumerable<Rule> RulesPerRealm(this DzProject proj, DzModule mod, int realmId)
    {
        var deps = mod.Dependencies;
        for (int i = deps.Length - 1; i >= 0; i--)
        {
            var dep = deps[i];

            var ruleIds = proj.Ruleset(dep)._pRulesByRealm[realmId];
            for (int j = ruleIds.Count - 1; j >= 0; j--)
            {
                var id = ruleIds[j];
                var rule = proj.GetRule(new(dep.AssemblyId, dep.LocalId, id));
                yield return rule;
            }
        }
    }
    public static IEnumerable<Rule> RulesPerClass(this DzProject proj, DzModule mod, RuleId ruleClass)
    {
        var deps = mod.Dependencies;
        for (int i = deps.Length - 1; i >= 0; i--)
        {
            var dep = deps[i];
            if (dep.AssemblyId < ruleClass.AssemblyId)
                continue;

            var ruleIds = proj.Ruleset(dep)._pRulesByClass[ruleClass];
            for (int j = ruleIds.Count - 1; j >= 0; j--)
            {
                var (isClass, id) = ruleIds[j];
                if (isClass)
                    foreach (var rule in proj.RulesPerClass(mod, new(dep.AssemblyId, -1, id)))
                        yield return rule;
                else
                    yield return proj.GetRule(new(dep.AssemblyId, dep.LocalId, id));
            }
        }
    }
}