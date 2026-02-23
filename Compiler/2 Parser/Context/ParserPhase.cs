using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface INodeAttrs
{
    //ATTRIBUTES
    public void StoreAttr(string attr);
    public bool HasAttr(string attr);
    public bool HasAttr(int nodeId, string attr);

    public bool TryFindAttr(string attr, out int nodeId);
    public int FindAttr(string attr);

    public bool TryResolveAttr<R>(string attr, [NotNullWhen(true)] out R? inst)
    where R : RuleInstance;
    public R ResolveAttr<R>(string attr)
    where R : RuleInstance;

    public bool TryResolveAttr(string attr, [NotNullWhen(true)] out RuleInstance? inst);
    public RuleInstance ResolveAttr(string attr);
}
public interface INodeTags
{
    //TAGS
    public void StoreTag(string tagType, string tag);
    public bool HasTag(string tagType, string tag);

    public bool TryFindTag(string tagType, string tag, out int nodeId);
    public int FindTag(string tagType, string tag);

    public bool TryResolveTag<R>(string tagType, string tag, [NotNullWhen(true)] out R? inst)
    where R : RuleInstance;
    public R ResolveTag<R>(string tagType, string tag)
    where R : RuleInstance;

    public bool TryResolveTag(string tagType, string tag, [NotNullWhen(true)] out RuleInstance? inst);
    public RuleInstance ResolveTag(string tagType, string tag);
}

public partial class ParserProcess : INodeAttrs, INodeTags
{
    private RuleInstance? RuleInst;

    //ATTRIBUTES
    public void StoreAttr(string attr)
    => Site.StoreAttr(RuleInst!.NodeId, attr);

    public bool HasAttr(string attr)
    => TryFindAttr(attr, out _);
    public bool HasAttr(int nodeId, string attr)
    => Site.HasAttr(nodeId, attr);

    public bool TryFindAttr(string attr, out int nodeId)
    {
        bool hasScopeVar(int nodeId, out int id)
        {
            id = nodeId;
            return Site.HasAttr(nodeId, attr);
        }

        bool findVarInSibling(int nodeId, int limit, out int id)
        {
            if (nodeId != limit && TAST.TryNodeAt(nodeId, out var node))
            {
                if (findVarInSibling(node.NextSiblingId, limit, out id)
                || findVarInChildren(node, out id))
                    return true;
            }
            id = 0;
            return false;
        }
        bool findVarInChildren(in TASTNode node, out int id)
        {
            if (!node.Args.IsScoped)
            {
                var childId = node.FirstChildId;
                while (TAST.TryNodeAt(childId, out var child))
                {
                    if (findVarInChildren(child, out id))
                        return true;
                    childId = child.NextSiblingId;
                }
            }
            return hasScopeVar(node.Id, out id);
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
    public int FindAttr(string attr)
    {
        if (!TryFindAttr(attr, out var nodeId))
            throw new Exception($"ATTRIBUTE NOT FOUND: attr={attr}");

        return nodeId;
    }

    public bool TryResolveAttr<R>(string attr, [NotNullWhen(true)] out R? inst)
    where R : RuleInstance
    {
        inst = null;
        if (!TryFindAttr(attr, out var nodeId) || !Site._ruleAppliance.TryGetValue(nodeId, out var rinst))
            return false;

        Debug.Assert(rinst is R);
        inst = (R)rinst;
        return true;
    }
    public R ResolveAttr<R>(string attr)
    where R : RuleInstance
    {
        if (!Site._ruleAppliance.TryGetValue(FindAttr(attr), out var inst))
            throw new Exception($"ATTRIBUTE INSTANCE NOT FOUND: attr={attr}");

        Debug.Assert(inst is R);
        return (R)inst;
    }

    public bool TryResolveAttr(string attr, [NotNullWhen(true)] out RuleInstance? inst)
    => TryResolveAttr<RuleInstance>(attr, out inst);
    public RuleInstance ResolveAttr(string attr)
    => ResolveAttr<RuleInstance>(attr);

    //TAGS
    public void StoreTag(string tagType, string tag)
    => Site.StoreTag(RuleInst!.NodeId, tagType, tag);
    public bool HasTag(string tagType, string tag)
    => Site.TryFindTag(tagType, tag, out _);

    public bool TryFindTag(string tagType, string tag, out int nodeId)
    => Site.TryFindTag(tagType, tag, out nodeId);
    public int FindTag(string tagType, string tag)
    {
        if (!TryFindTag(tagType, tag, out var nodeId))
            throw new Exception($"TAG NOT FOUND: tagType={tagType}, tag={tag}");

        return nodeId;
    }

    public bool TryResolveTag<R>(string tagType, string tag, [NotNullWhen(true)] out R? inst)
    where R : RuleInstance
    {
        inst = null;
        if (!TryFindTag(tagType, tag, out var nodeId) || !Site._ruleAppliance.TryGetValue(nodeId, out var rinst))
            return false;

        Debug.Assert(rinst is R);
        inst = (R)rinst;
        return true;
    }
    public R ResolveTag<R>(string tagType, string tag)
    where R : RuleInstance
    {
        if (!Site._ruleAppliance.TryGetValue(FindTag(tagType, tag), out var inst))
            throw new Exception($"TAG INSTANCE NOT FOUND: tagType={tagType}, tag={tag}");

        Debug.Assert(inst is R);
        return (R)inst;
    }

    public bool TryResolveTag(string tagType, string tag, [NotNullWhen(true)] out RuleInstance? inst)
    => TryResolveTag<RuleInstance>(tagType, tag, out inst);
    public RuleInstance ResolveTag(string tagType, string tag)
    => ResolveTag<RuleInstance>(tagType, tag);
}