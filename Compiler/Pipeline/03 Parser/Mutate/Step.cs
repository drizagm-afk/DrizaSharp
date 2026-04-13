using System.Collections.Immutable;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public enum Pass { Build, Bind, BindData, Validate, Emit }
public partial class ParserProcess
{
    private Pass _curPass;

    //>>>> MUTATE PROJECT <<<<
    private void Mutate()
    {
        foreach (var file in Project.Files)
            Mutate(file);
    }

    //>>>> MUTATE FILE <<<<
    private void Mutate(DzFile file)
    {
        File = file;
        Mutate(TAST.Root);
    }
    private void Mutate(in TASTNode node, bool includeNode = true)
    {
        if (includeNode && TAST.TryGetApplyRule(node.Id, out var inst))
        {
            _tokenCount = TAST.TokenCount;
            _tokenNodes.Clear();
            _evalRules.Clear();

            if (_curPass == Pass.Build)
                inst.BuildMutate(this);
            else if (_curPass == Pass.Bind)
                inst.BindMutate(this);
            else if (_curPass == Pass.BindData)
                inst.BindDataMutate(this);
            else if (_curPass == Pass.Validate)
                inst.ValidateMutate(this);
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
    private void ApplyRecompile(int nodeId, bool includeNode)
    {
        ref readonly var node = ref TAST.NodeAt(nodeId);
        var upToPass = _curPass;

        Recompile(node, includeNode, upTo: upToPass);

        _curPass = upToPass;
    }
    public delegate void TASTNodeAction(in TASTNode node);
    private void Recompile(in TASTNode node, bool includeNode, Pass upTo)
    {
        if (!includeNode && node.IsFlat())
            return;

        void recompile(in TASTNode node, TASTNodeAction function)
        {
            InitTagsMemory();
            RecomputeTags(node, includeNode);

            if (includeNode)
                function(node);
            else
            {
                var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
                while (childExists)
                {
                    function(child);
                    childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                }
            }

            ClearTagsMemory();
        }

        //BUILD
        _curPass = Pass.Build;
        Mutate(node, includeNode);

        //BIND
        _curPass = Pass.Bind;
        if (upTo < _curPass)
            return;

        recompile(node, RecompileBind);
        Mutate(node, includeNode);

        //BIND DATA
        _curPass = Pass.BindData;
        if (upTo < _curPass)
            return;

        recompile(node, RecompileBind);
        Mutate(node, includeNode);

        //VALIDATE
        _curPass = Pass.Validate;
        if (upTo < _curPass)
            return;

        recompile(node, RecompileValidate);
        Mutate(node, includeNode);
    }
    private void RecompileBind(in TASTNode node)
    => Bind(node);
    private void RecompileValidate(in TASTNode node)
    => Validate(node);

    //==== RECOMPUTE TAGS ====
    private void RecomputeTags(in TASTNode node, bool excludeNode = true)
    {
        var isScoped = TAST.InfoAt(node.Id).IsScoped;
        if (isScoped) EnterScope();

        if (!excludeNode)
            RestoreTags(node.Id);

        RecomputeChildrenTags(node.FirstChildId);

        if (isScoped) ExitScope();
    }
    private void RecomputeChildrenTags(int firstChildId)
    {
        var childExists = TAST.TryNodeAt(firstChildId, out var child);
        while (childExists)
        {
            RecomputeTags(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }
    }
    private void RestoreTags(int nodeId)
    {
        if (_persistTags.TryGetValue(new(_curPass, nodeId), out var vals))
        {
            foreach (var val in vals)
            {
                if (!_scope.TryGetValue(val, out var stack))
                    stack = _scope[val] = [];

                stack.Add(nodeId);
                _scopeFrames[^1].Add(val);
            }
        }
    }
}