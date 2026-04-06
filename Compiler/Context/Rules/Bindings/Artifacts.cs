namespace DrzSharp.Compiler.Rules;

public interface ArtifactsBinding
{
    //>>>> STATE <<<<
    public S State<S>() where S : State, new();

    //>>>> CUSTOM RULESET <<<<
    public RuleId BindToRuleset<T>(T item, CustomRuleset<T> ruleset);
}

internal partial class RulesetBinding : ArtifactsBinding
{
    //>>>> STATE <<<<
    public S State<S>() where S : State, new()
    {
        var type = typeof(S);
        if (!_ruleset._states.TryGetValue(type, out var state))
            _ruleset._states[typeof(S)] = state = new S();

        return (S)state;
    }

    //>>>> CUSTOM RULESET <<<<
    public RuleId BindToRuleset<T>(T item, CustomRuleset<T> ruleset)
    {
        var localRuleset = ruleset._ruleset[_assemblyId][_nspaceId];
        var ruleId = NewRuleId(localRuleset.Count);
        localRuleset.Add(item);

        return ruleId;
    }
}