namespace DrzSharp.Compiler.Parser;

public interface ValidateContext : Context, INodeAttrs, INodeTags
{
    public void Abort(string message = "TEXT SPAN IS INVALID");

    public void Validate(RuleInstance inst);
    public void Validate(int nodeId);
}
public partial class ParserProcess : ValidateContext
{
    public void Abort(string message)
    => throw new Exception(message);

    public void Validate(RuleInstance inst)
    => Validate(inst.NodeId);
    public void Validate(int nodeId)
    {
        var caller = RuleInst;
        Validate(TAST.NodeAt(nodeId));
        RuleInst = caller;
    }
}