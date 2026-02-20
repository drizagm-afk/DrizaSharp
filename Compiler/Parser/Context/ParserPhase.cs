using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public interface PassContext
{
    public void StoreSVar(string varType, string var);
    public bool HasSVar(string varType, string var);

    public bool TryResolveSVar(string varType, string var, out int nodeId);
    public int ResolveSVar(string varType, string var);

    public bool TryResolveSVarInst<R>(string varType, string var, [NotNullWhen(true)] out R? inst) 
    where R : RuleInstance;
    public R ResolveSVarInst<R>(string varType, string var)
    where R : RuleInstance;

    public bool TryResolveSVarInst(string varType, string var, [NotNullWhen(true)] out RuleInstance? inst);
    public RuleInstance ResolveSVarInst(string varType, string var);
}
public partial class ParserProcess : PassContext
{
    private RuleInstance? RuleInst;

    public void StoreSVar(string varType, string var)
    => Site.StoreScopeVar(RuleInst!.NodeId, varType, var);

    public bool HasSVar(string varType, string var)
    => TryResolveSVar(varType, var, out _);

    public bool TryResolveSVar(string varType, string var, out int nodeId)
    {
        bool hasScopeVar(int nodeId, out int id)
        {
            id = nodeId;
            return Site.HasScopeVar(nodeId, varType, var);
        }

        bool findVarInSibling(int nodeId, int limit, out int id)
        {
            if (nodeId != limit && TAST.TryNodeAt(nodeId, out var node))
            {
                if (findVarInSibling(node.NextSiblingId, limit, out id) 
                || hasScopeVar(nodeId, out id))
                    return true;
            }
            id = 0;
            return false;
        }
        bool findVarInNode(in TASTNode node, out int id)
        {
            if (hasScopeVar(node.Id, out id))
                return true;
            else if (node.Id != Site.RootId && TAST.TryNodeAt(node.ParentId, out var parent))
            {
                if (findVarInSibling(parent.FirstChildId, node.Id, out id) 
                || findVarInNode(parent, out id))
                    return true;
            }
            return false;
        }

        return findVarInNode(TAST.NodeAt(RuleInst!.NodeId), out nodeId);
    }
    public int ResolveSVar(string varType, string var)
    {
        if (!TryResolveSVar(varType, var, out var nodeId))
            throw new Exception($"SCOPE VAR NOT FOUND: varType={varType}, var={var}");

        return nodeId;
    }

    public bool TryResolveSVarInst<R>(string varType, string var, [NotNullWhen(true)] out R? inst) 
    where R : RuleInstance
    {
        inst = null;
        if (!TryResolveSVar(varType, var, out var nodeId) || !Site._ruleAppliance.TryGetValue(nodeId, out var rinst))
            return false;

        Debug.Assert(rinst is R);
        inst = (R)rinst;
        return true;
    }
    public R ResolveSVarInst<R>(string varType, string var) 
    where R : RuleInstance
    {
        if (!Site._ruleAppliance.TryGetValue(ResolveSVar(varType, var), out var inst))
            throw new Exception($"SCOPE VAR INST NOT FOUND: varType={varType}, var={var}");

        Debug.Assert(inst is R);
        return (R)inst;
    }

    public bool TryResolveSVarInst(string varType, string var, [NotNullWhen(true)] out RuleInstance? inst)
    => TryResolveSVarInst<RuleInstance>(varType, var, out inst);
    public RuleInstance ResolveSVarInst(string varType, string var)
    => ResolveSVarInst<RuleInstance>(varType, var);
}