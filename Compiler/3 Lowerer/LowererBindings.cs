namespace DrzSharp.Compiler.Lowerer;

public static class Binding
{
    public static void BindRule(Rule rule, out RuleId ruleId)
    {
        ruleId = new(LowererManager._rules.Count);
        LowererManager._rules.Add(rule);
    }
}