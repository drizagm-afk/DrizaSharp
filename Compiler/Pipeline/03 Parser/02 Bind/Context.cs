using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

//>>>> BIND <<<<
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

//>>>> BIND DATA <<<<
public interface BindDataContext : SemanticContext
{
    //RECURSIVE BINDING
    public void BindData(RuleInstance inst);
    public void BindData(int nodeId);
    public void BindData(params int[] nodeIds);
}
public partial class ParserProcess : BindContext
{
    //RECURSIVE BINDING
    public void BindData(RuleInstance inst)
    {
        var caller = RuleInst;
        BindData(TAST.NodeAt(inst.NodeId), inst);
        RuleInst = caller;
    }
    public void BindData(int nodeId)
    {
        var caller = RuleInst;
        BindData(TAST.NodeAt(nodeId));
        RuleInst = caller;
    }
    public void BindData(int[] nodeIds)
    {
        foreach(var id in nodeIds)
            BindData(id);
    }
}