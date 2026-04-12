using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> BIND PROJECT <<<<
    public partial void Bind()
    {
        _curPass = Pass.Bind;
        foreach (var file in Project.Files)
            Bind(file);
        Mutate();

        _curPass = Pass.BindData;
        foreach (var file in Project.Files)
            BindData(file);
        Mutate();
    }

    //>>>> BIND FILE <<<<
    private void Bind(DzFile file)
    {
        File = file;

        InitTagsMemory();
        Bind(TAST.Root);
        ClearTagsMemory();
    }
    private bool Bind(in TASTNode node)
    {
        if (TAST.TryGetApplyRule(node.Id, out var inst))
            return Bind(node, inst);

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        var isChildrenValid = BindChildren(node.FirstChildId);

        if (isScoped) ExitScope();

        return isChildrenValid;
    }
    private bool Bind(in TASTNode node, RuleInstance inst)
    {
        RuleInst = inst;

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        try { inst.Bind(this); }
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
        if (!BindChildren(node.FirstChildId))
            inst.Validity = Validity.Invalid;

        if (isScoped) ExitScope();

        //APPLY VALIDITY
        if (inst.Validity == Validity.Invalid)
            return false;

        inst.Validity = Validity.Valid;
        return true;
    }
    private bool BindChildren(int firstChildId)
    {
        var childExists = TAST.TryNodeAt(firstChildId, out var child);
        bool isValid = true;

        while (childExists)
        {
            isValid &= Bind(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
        return isValid;
    }

    //>>>> BIND FILE DATA <<<<
    private void BindData(DzFile file)
    {
        File = file;

        InitTagsMemory();
        BindData(TAST.Root);
        ClearTagsMemory();
    }
    private bool BindData(in TASTNode node)
    {
        if (TAST.TryGetApplyRule(node.Id, out var inst))
            return BindData(node, inst);

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        var isChildrenValid = BindChildrenData(node.FirstChildId);

        if (isScoped) ExitScope();

        return isChildrenValid;
    }
    private bool BindData(in TASTNode node, RuleInstance inst)
    {
        RuleInst = inst;

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        try { inst.Bind(this); }
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
        if (!BindChildrenData(node.FirstChildId))
            inst.Validity = Validity.Invalid;

        if (isScoped) ExitScope();

        //APPLY VALIDITY
        if (inst.Validity == Validity.Invalid)
            return false;

        inst.Validity = Validity.Valid;
        return true;
    }
    private bool BindChildrenData(int firstChildId)
    {
        var childExists = TAST.TryNodeAt(firstChildId, out var child);
        bool isValid = true;

        while (childExists)
        {
            isValid &= Bind(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
        return isValid;
    }
}