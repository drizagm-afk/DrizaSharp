using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface ValidateContext : Context, SemanticContext
{
    //DIAGNOSTICS
    public void AddError(string message = "Invalid Text Span");
    public void AddWarning(string message);
    public void AddInfo(string message);

    public void Abort(string message = "Invalid Text Span");
    public void AbortIfError();

    //RECURSIVE VALIDATION
    public bool Validate(RuleInstance inst);
    public bool Validate(int nodeId);
    public bool Validate(params int[] nodeIds);
}
public partial class ParserProcess : ValidateContext
{
    //DIAGNOSTICS
    public void AddError(string message)
    {
        RuleInst!.Validity = Validity.Invalid;

        var nodeId = RuleInst.NodeId;
        Diagnostics.AddError(
            TAST.SourceSlice(nodeId),
            nodeId,
            message
        );
    }
    public void AddWarning(string message)
    {
        var nodeId = RuleInst!.NodeId;
        Diagnostics.AddWarning(
            TAST.SourceSlice(nodeId),
            nodeId,
            message
        );
    }
    public void AddInfo(string message)
    {
        var nodeId = RuleInst!.NodeId;
        Diagnostics.AddInfo(
            TAST.SourceSlice(nodeId),
            nodeId,
            message
        );
    }

    public void Abort(string message)
    {
        AddError(message);
        throw new AbortException();
    }
    public void AbortIfError()
    {
        if (RuleInst!.Validity == Validity.Invalid)
            throw new AbortException();
    }

    //RECURSIVE VALIDATION
    public bool Validate(RuleInstance inst)
    {
        var caller = RuleInst;
        var isValid = Validate(TAST.NodeAt(inst.NodeId), inst);
        RuleInst = caller;

        return isValid;
    }
    public bool Validate(int nodeId)
    {
        var caller = RuleInst;
        var isValid = Validate(TAST.NodeAt(nodeId));
        RuleInst = caller;

        return isValid;
    }
    public bool Validate(int[] nodeIds)
    {
        var isValid = true;
        foreach (var id in nodeIds)
            isValid &= Validate(id);
        
        return isValid;
    }
}