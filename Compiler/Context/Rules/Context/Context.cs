namespace DrzSharp.Compiler.Rules;

internal static class RulesetExt
{
    //>>>> GLOBAL <<<<
    internal static Ruleset Ruleset(this DzProject proj)
    {
        var cctx = CompilationContext.ContextAt(proj.Id);
        return cctx.GetRuleset();
    }

    internal static LocalRuleset Ruleset(this DzProject proj, RuleId ruleId)
    => proj.Ruleset(ruleId.AssemblyId, ruleId.NspaceId);
    internal static LocalRuleset Ruleset(this DzProject proj, GlobalId globalId)
    => proj.Ruleset(globalId.AssemblyId, globalId.LocalId);
    internal static LocalRuleset Ruleset(this DzProject proj, int assemblyId, int nspaceId)
    => proj.Ruleset()._localRulesets[assemblyId][nspaceId];

    //>>>> ARTIFACTS <<<<
    internal static S State<S>(this DzProject proj) where S : State
    => (S)proj.Ruleset()._states[typeof(S)];

    public static IEnumerable<T> CustomRules<T>(this DzProject proj, DzModule module, CustomRuleset<T> ruleset, RuleOrder order = RuleOrder.NewestToOldest)
    {
        var deps = module.Dependencies;
        if (order == RuleOrder.NewestToOldest)
        {
            for (int i = deps.Length - 1; i >= 0; i--)
            {
                var dep = deps[i];
                var rules = ruleset._ruleset[dep.AssemblyId][dep.LocalId];

                for (int j = rules.Count - 1; j >= 0; j--)
                    yield return rules[j];
            }
        }
        else
        {
            for (int i = 0; i < deps.Length; i++)
            {
                var dep = deps[i];
                var rules = ruleset._ruleset[dep.AssemblyId][dep.LocalId];

                for (int j = 0; j < rules.Count; j++)
                    yield return rules[j];
            }
        }
    }
}