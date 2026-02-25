namespace DrzSharp.Compiler.Lowerer;

public static class Binding
{
    public static int BindRule(Rule rule)
    {
        int id = LowererManager._rules.Count;
        LowererManager._rules.Add(rule);

        return id;
    }
}