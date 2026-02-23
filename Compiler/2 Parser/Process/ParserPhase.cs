using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //PARSE SITES
    private ParserSite Site = null!;
    private TAST TAST => Project.Files[Site.FileId].TAST;
}

public partial class ParserSite
{
    /*
    private readonly HashSet<ScopeKey> _scope = [];
    public bool StoreScopeVar(int nodeId, string varType, string var)
    => _scope.Add(new(nodeId, varType, var));
    public bool HasScopeVar(int nodeId, string varType, string var)
    => _scope.Contains(new(nodeId, varType, var));
    */
    //ATTRIBUTES
    private readonly HashSet<AttrKey> _attributes = [];
    public bool StoreAttr(int nodeId, string attr)
    => _attributes.Add(new(nodeId, attr));
    public bool HasAttr(int nodeId, string attr)
    => _attributes.Contains(new(nodeId, attr));

    //TAGS
    private readonly Dictionary<TagKey, Stack<int>> _scope = [];
    private readonly Stack<List<TagKey>> _scopeFrames = [];
    internal void EnterScope()
    => _scopeFrames.Push([]);
    internal void ExitScope()
    {
        var frame = _scopeFrames.Pop();
        foreach(var key in frame)
        {
            var stack = _scope[key];
            stack.Pop();

            if (stack.Count == 0)
                _scope.Remove(key);
        }
    }    

    internal void StoreTag(int nodeId, string tagType, string tag)
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
    internal bool TryFindTag(string tagType, string tag, out int nodeId)
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
}
internal readonly record struct AttrKey
(int NodeId, string Attr);
internal readonly record struct TagKey
(string TagType, string Tag);