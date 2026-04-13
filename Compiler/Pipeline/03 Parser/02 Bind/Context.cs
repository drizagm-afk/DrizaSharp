using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface BindContext : SemanticContext
{
    //RECURSIVE BINDING
    public void Bind(RuleInstance inst);
    public void Bind(int nodeId);
    public void Bind(params int[] nodeIds);
}
public partial class ParserProcess : BindContext
{
    //RECURSIVE BINDING
    public void Bind(RuleInstance inst)
    {
        var caller = RuleInst;
        Bind(TAST.NodeAt(inst.NodeId), inst);
        RuleInst = caller;
    }
    public void Bind(int nodeId)
    {
        var caller = RuleInst;
        Bind(TAST.NodeAt(nodeId));
        RuleInst = caller;
    }
    public void Bind(int[] nodeIds)
    {
        foreach(var id in nodeIds)
            Bind(id);
    }
}