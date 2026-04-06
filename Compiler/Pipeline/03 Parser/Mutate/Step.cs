using System.Collections.Immutable;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    public enum Pass { Build, Bind, BindData, Validate }
    private Pass _lastPhase;
    private Pass _curPhase;

    //>>>> MUTATE PROJECT <<<<
    private void Mutate(Pass phase)
    {
        _lastPhase = phase;
        _curPhase = phase;
        foreach (var file in Project.Files)
            Mutate(file);
    }

    //>>>> MUTATE FILE <<<<
    private void Mutate(DzFile file)
    {
        File = file;
        Mutate(TAST.Root);
    }
    private void Mutate(in TASTNode node)
    {
        if (TAST.TryGetApplyRule(node.Id, out var inst))
        {
            _tokenCount = TAST.TokenCount;
            _tokenNodes.Clear();
            _evalRules.Clear();

            if (_curPhase == Pass.Build)
                inst.BuildMutate(this);
            else if (_curPhase == Pass.Bind)
                inst.BindMutate(this);
            else if (_curPhase == Pass.BindData)
                inst.BindDataMutate(this);
            else if (_curPhase == Pass.Validate)
                inst.ValidateMutate(this);

            if (inst.Rewritten)
                return;
        }

        MutateChildren(node);
    }
    private void MutateChildren(in TASTNode node)
    {
        if (node.IsFlat()) return;

        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            Mutate(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }

    //>>>> RECOMPILATION <<<<
    private void ApplyMutate(int nodeId, ImmutableArray<RuleId> evalRules)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);

        //MATCH
        if (evalRules.Length == 0)
            Match(node);
        else
            Match(node, evalRules);
    }
    private void ApplyRecompile(int nodeId, bool recompileNode)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);
        var srtPhase = _curPhase;

        //START
        if (recompileNode)
            Recompile(node);
        else
            RecompileChildren(node);

        //END
        _curPhase = srtPhase;
    }
    private void Recompile(in TASTNode node)
    {
        _curPhase = Pass.Build;
        Mutate(node);

        //BIND
        _curPhase = Pass.Bind;
        if (_lastPhase < _curPhase)
            return;

        Bind(node);
        Mutate(node);

        //BIND DATA
        _curPhase = Pass.BindData;
        if (_lastPhase < _curPhase)
            return;

        BindData(node);
        Mutate(node);

        //VALIDATE
        _curPhase = Pass.Validate;
        if (_lastPhase < _curPhase)
            return;

        Validate(node);
        Mutate(node);
    }
    private void RecompileChildren(in TASTNode node)
    {
        if (node.IsFlat()) return;

        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            Recompile(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }
}