namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    private void BuildRule(RuleInstance inst) => NestRule(inst);
    private void EndBuild() => RuleInst = null;
}

public partial class ParserSite
{
    internal readonly Dictionary<int, RuleInstance> _ruleAppliance = [];
}