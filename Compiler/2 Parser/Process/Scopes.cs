using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //SCOPED TAGS
    private readonly Dictionary<TagKey, Stack<int>> _scope = [];
    private readonly Stack<List<TagKey>> _scopeFrames = [];
    private void EnterScope()
    => _scopeFrames.Push([]);
    private void ExitScope()
    {
        var frame = _scopeFrames.Pop();
        foreach (var key in frame)
        {
            var stack = _scope[key];
            stack.Pop();

            if (stack.Count == 0)
                _scope.Remove(key);
        }
    }
    private void StoreScopedTag(int nodeId, string tagType, string tag)
    {
        TagKey key = new(tagType, tag);

        if (!_scope.TryGetValue(key, out var stack))
        {
            stack = new Stack<int>();
            _scope[key] = stack;
        }
        stack.Push(nodeId);
        _scopeFrames.Peek().Add(key);
    }
    private bool TryFindScopedTag(string tagType, string tag, out int nodeId)
    {
        TagKey key = new(tagType, tag);

        if (_scope.TryGetValue(key, out var stack) && stack.Count > 0)
        {
            nodeId = stack.Peek();
            return true;
        }
        nodeId = -1;
        return false;
    }

    //SCOPED ATTRIBUTES
    private bool TryFindScopedAttr(string attr, out int nodeId)
    {
        bool hasScopeVar(int nodeId, out int id)
        {
            id = nodeId;
            return TAST.HasAttr(nodeId, attr);
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
            if (!TAST.InfoAt(node.Id).IsScoped)
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
            else if (TAST.TryNodeAt(node.ParentId, out var parent))
            {
                if (findVarInSibling(parent.FirstChildId, node.Id, out id)
                || findVarInNode(parent, out id))
                    return true;
            }
            return false;
        }
        return findVarInNode(TAST.NodeAt(RuleInst!.NodeId), out nodeId);
    }
}
internal readonly record struct TagKey
(string TagType, string Tag);