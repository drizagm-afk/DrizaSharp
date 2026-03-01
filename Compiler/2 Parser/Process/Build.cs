namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    private void StartBuild(RuleInstance inst) => NestRule(inst);
    private void EndBuild() => RuleInst = null;
}