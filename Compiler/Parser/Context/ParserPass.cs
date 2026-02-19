using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public interface ParserPassContext
{
    public void StoreVar(string varType, string var);
    public bool HasVar(string varType, string var);

    public bool TryLoadVarNode(string varType, string var, out int nodeId);
    public int LoadVarNode(string varType, string var);

    public bool TryLoadVarInst<R>(string varType, string var, [NotNullWhen(true)] out R? inst) 
    where R : ParserRuleInstance;
    public R LoadVarInst<R>(string varType, string var) 
    where R : ParserRuleInstance;

    public bool TryLoadVarInst(string varType, string var, [NotNullWhen(true)] out ParserRuleInstance? inst);
    public ParserRuleInstance LoadVarInst(string varType, string var);
}
public partial class ParserProcess : ParserPassContext
{
    private ParserRuleInstance? RuleInst;

    public void StoreVar(string varType, string var)
    => Site.StoreScopeVar(RuleInst!.NodeId, varType, var);

    public bool HasVar(string varType, string var)
    => TryLoadVarNode(varType, var, out _);

    public bool TryLoadVarNode(string varType, string var, out int nodeId)
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
    public int LoadVarNode(string varType, string var)
    {
        if (!TryLoadVarNode(varType, var, out var nodeId))
            throw new Exception("");

        return nodeId;
    }

    public bool TryLoadVarInst<R>(string varType, string var, [NotNullWhen(true)] out R? inst) 
    where R : ParserRuleInstance
    {
        inst = null;
        if (!TryLoadVarNode(varType, var, out var nodeId) || !Site._ruleAppliance.TryGetValue(nodeId, out var rinst))
            return false;

        Debug.Assert(rinst is R);
        inst = (R)rinst;
        return true;
    }
    public R LoadVarInst<R>(string varType, string var) 
    where R : ParserRuleInstance
    {
        if (!Site._ruleAppliance.TryGetValue(LoadVarNode(varType, var), out var inst))
            throw new Exception("");

        Debug.Assert(inst is R);
        return (R)inst;
    }

    public bool TryLoadVarInst(string varType, string var, [NotNullWhen(true)] out ParserRuleInstance? inst)
    => TryLoadVarInst<ParserRuleInstance>(varType, var, out inst);
    public ParserRuleInstance LoadVarInst(string varType, string var)
    => LoadVarInst<ParserRuleInstance>(varType, var);
}