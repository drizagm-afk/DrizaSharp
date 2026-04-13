using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> BIND PROJECT <<<<
    public partial bool Bind()
    {
        _curPass = Pass.Bind;
        foreach (var file in Project.Files)
            Bind(file);
        Mutate();

        if (HasError())
            return false;

        _curPass = Pass.BindData;
        foreach (var file in Project.Files)
            Bind(file);
        Mutate();

        return !HasError();
    }

    //>>>> BIND FILE <<<<
    private void Bind(DzFile file)
    {
        File = file;

        InitTagsMemory();
        Bind(TAST.Root);
        ClearTagsMemory();
    }
    private void Bind(in TASTNode node)
    {
        if (TAST.TryGetApplyRule(node.Id, out var inst))
        {
            Bind(node, inst);
            return;
        }

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        BindChildren(node.FirstChildId);

        if (isScoped) ExitScope();
    }
    private void Bind(in TASTNode node, RuleInstance inst)
    {
        RuleInst = inst;

        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        try
        {
            if (_curPass == Pass.Bind)
                inst.Bind(this);
            else if (_curPass == Pass.BindData)
                inst.BindData(this);
        }
        catch (Exception e)
        {
            Diagnostics.ReportUnhandled(
                TAST.SourceSlice(node),
                inst.NodeId,
                $"({e.GetType().Name}) {e.Message}"
            );
        }
        BindChildren(node.FirstChildId);

        if (isScoped) ExitScope();
    }
    private void BindChildren(int firstChildId)
    {
        var childExists = TAST.TryNodeAt(firstChildId, out var child);
        while (childExists)
        {
            Bind(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }
}