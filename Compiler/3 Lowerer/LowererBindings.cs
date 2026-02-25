namespace DrzSharp.Compiler.Lowerer;

public static class Binding
{
    public static RuleId BindRule(Rule rule)
    {
        RuleId ruleId = new(LowererManager._rules.Count);
        LowererManager._rules.Add(rule);

        return ruleId;
    }
}