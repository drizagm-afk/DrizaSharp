using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> VALIDATE PROJECT <<<<
    public partial void Validate()
    {
        foreach (var file in Project.Files)
            Validate(file);
        Mutate(Pass.Validate);
    }

    //>>>> VALIDATE FILE <<<<
    public void Validate(DzFile file)
    {
        File = file;

        InitTagsMemory();
        Validate(TAST.Root);
        ClearTagsMemory();
    }
    private bool Validate(in TASTNode node)
    {
        if (TAST.TryGetApplyRule(node.Id, out var inst))
            return Validate(node, inst);

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        var isChildrenValid = ValidateChildren(node.FirstChildId);

        if (isScoped) ExitScope();

        return isChildrenValid;
    }
    private bool Validate(in TASTNode node, RuleInstance inst)
    {
        RuleInst = inst;

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        try { inst.Validate(this); }
        catch (AbortException) { }
        catch (Exception e)
        {
            inst.Validity = Validity.Invalid;
            Diagnostics.ReportUnhandled(
                TAST.SourceSlice(node),
                inst.NodeId,
                $"({e.GetType().Name}) {e.Message}"
            );
        }
        if (!ValidateChildren(node.FirstChildId))
            inst.Validity = Validity.Invalid;

        if (isScoped) ExitScope();

        //APPLY VALIDITY
        if (inst.Validity == Validity.Invalid)
            return false;

        inst.Validity = Validity.Valid;
        return true;
    }
    private bool ValidateChildren(int firstChildId)
    {
        var childExists = TAST.TryNodeAt(firstChildId, out var child);
        bool isValid = true;

        while (childExists)
        {
            isValid &= Validate(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
        return isValid;
    }
}