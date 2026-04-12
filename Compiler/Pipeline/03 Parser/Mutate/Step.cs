using System.Collections.Immutable;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    public enum Pass { Build, Bind, BindData, Validate, Emit }
    private Pass _curPass;
    private Pass _curMutatePass;

    //>>>> MUTATE PROJECT <<<<
    private void Mutate()
    {
        _curMutatePass = _curPass;
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

            if (_curMutatePass == Pass.Build)
                inst.BuildMutate(this);
            else if (_curMutatePass == Pass.Bind)
                inst.BindMutate(this);
            else if (_curMutatePass == Pass.BindData)
                inst.BindDataMutate(this);
            else if (_curMutatePass == Pass.Validate)
                inst.ValidateMutate(this);

            if (inst.IsRewritten)
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
        var srtPass = _curMutatePass;

        //START
        if (recompileNode)
            Recompile(node);
        else
            RecompileChildren(node);

        //END
        _curMutatePass = srtPass;
    }
    private void Recompile(in TASTNode node)
    {
        _curMutatePass = Pass.Build;
        Mutate(node);

        //BIND
        _curMutatePass = Pass.Bind;
        if (_curPass < _curMutatePass)
            return;

        InitTagsMemory();
        Bind(node);
        ClearTagsMemory();
        Mutate(node);

        //BIND DATA
        _curMutatePass = Pass.BindData;
        if (_curPass < _curMutatePass)
            return;

        InitTagsMemory();
        BindData(node);
        ClearTagsMemory();
        Mutate(node);

        //VALIDATE
        _curMutatePass = Pass.Validate;
        if (_curPass < _curMutatePass)
            return;

        InitTagsMemory();
        Validate(node);
        ClearTagsMemory();
        Mutate(node);
    }
    /*TO FIX, RESOLVE CHILDREN RECOMPILATION, MUST BE PARALLEL BETWEEN ALL CHILDREN*/
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

    /*TO FIX, RESOLVE RELOADING TAGS*/
    private void ReloadTags(in TASTNode node)
    {
        
    }
}