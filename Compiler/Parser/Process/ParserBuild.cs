namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    private void BuildRule(ParserRuleInstance inst) => NestRule(inst);
    private void EndBuild() => _buildCaller = null;
}

public partial class ParserSite
{
    internal readonly Dictionary<int, ParserRuleInstance> _ruleAppliance = [];
}