namespace DrzSharp.Compiler.Rules;

//>>>> STATES <<<<
public abstract class State { }

//>>>> CUSTOM RULESETS <<<<
public class CustomRuleset<T>
{
    internal Dictionary<int, Dictionary<int, List<T>>> _ruleset = [];
}
public enum RuleOrder
{ NewestToOldest, OldestToNewest }