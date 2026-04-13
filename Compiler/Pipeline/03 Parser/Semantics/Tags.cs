using System.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

//>>>> TAGS <<<<
public interface Tags
{
    //STORE TAG
    public void StoreTag(string tag, string tagDesc);
    public void StoreTag(string tag) => StoreTag(tag, string.Empty);
    public void StoreOuterTag(string tag, string tagDesc);
    public void StoreOuterTag(string tag) => StoreOuterTag(tag, string.Empty);

    //HAS TAG
    public bool HasTag(string tag, string tagDesc);
    public bool HasTag(string tag) => HasTag(tag, string.Empty);

    //FIND TAG
    public bool TryFindTag(string tag, string tagDesc, out int nodeId);
    public bool TryFindTag(string tag, out int nodeId) => TryFindTag(tag, string.Empty, out nodeId);
    public int FindTag(string tag, string tagDesc);
    public int FindTag(string tag) => FindTag(tag, string.Empty);

    //RESOLVE TAG
    public bool TryResolveTag<R>(string tag, string tagDesc, out R inst) where R : RuleInstance;
    public bool TryResolveTag<R>(string tag, out R inst) where R : RuleInstance
    => TryResolveTag(tag, string.Empty, out inst);
    public R ResolveTag<R>(string tag, string tagDesc) where R : RuleInstance;
    public R ResolveTag<R>(string tag) where R : RuleInstance
    => ResolveTag<R>(tag, string.Empty);

    public bool TryResolveTag(string tag, string tagDesc, out RuleInstance inst)
    => TryResolveTag<RuleInstance>(tag, tagDesc, out inst);
    public bool TryResolveTag(string tag, out RuleInstance inst)
    => TryResolveTag<RuleInstance>(tag, out inst);
    public RuleInstance ResolveTag(string tag, string tagDesc)
    => ResolveTag<RuleInstance>(tag, tagDesc);
    public RuleInstance ResolveTag(string tag)
    => ResolveTag<RuleInstance>(tag);
}
public partial class ParserProcess : Tags
{
    private readonly Dictionary<TagValue, List<int>> _scope = [];
    private readonly List<List<TagValue>> _scopeFrames = [];

    private void EnterScope()
    => _scopeFrames.Add([]);
    private void ExitScope()
    {
        var frame = _scopeFrames[^1];
        _scopeFrames.RemoveLast();

        foreach (var key in frame)
        {
            var stack = _scope[key];
            stack.RemoveLast();

            if (stack.Count == 0)
                _scope.Remove(key);
        }
    }

    private void InitTagsMemory()
    => EnterScope();
    private void ClearTagsMemory()
    {
        ExitScope();
        _scope.Clear();
        _scopeFrames.Clear();
    }

    //STORE TAG
    public void StoreTag(string tag, string tagDesc)
    {
        TagValue val = new(tag, tagDesc);
        var nodeId = RuleInst!.NodeId;

        if (!_scope.TryGetValue(val, out var stack))
            stack = _scope[val] = [];

        stack.Add(nodeId);
        _scopeFrames[^1].Add(val);

        StorePTag(nodeId, val);
    }
    public void StoreOuterTag(string tag, string tagDesc)
    {
        TagValue val = new(tag, tagDesc);
        var nodeId = RuleInst!.NodeId;

        if (_scopeFrames.Count < 2)
            throw new Exception("There's no previous scope");

        if (!_scope.TryGetValue(val, out var stack))
        {
            stack = _scope[val] = [];
            stack.Add(nodeId);
        }
        else
        {
            int i = stack.Count;
            foreach (var _key in _scopeFrames[^1])
                if (_key == val) i--;

            stack.Insert(i, nodeId);
        }

        _scopeFrames[^2].Add(val);

        StoreOuterPTag(nodeId, val);
    }

    //HAS TAG
    public bool HasTag(string tag, string tagDesc)
    {
        TagValue val = new(tag, tagDesc);
        return _scope.TryGetValue(val, out var stack) && stack.Count > 0;
    }

    //FIND TAG
    public int FindTag(string tag, string tagDesc)
    {
        if (!TryFindTag(tag, tagDesc, out var nodeId))
            throw new Exception($"TAG NOT FOUND: tag={tag}, tagDesc={tagDesc}");

        return nodeId;
    }
    public bool TryFindTag(string tag, string tagDesc, out int nodeId)
    {
        TagValue val = new(tag, tagDesc);

        if (_scope.TryGetValue(val, out var stack) && stack.Count > 0)
        {
            nodeId = stack[^1];
            return true;
        }
        nodeId = -1;
        return false;
    }

    //RESOLVE TAG
    public bool TryResolveTag<R>(string tag, string tagDesc, out R inst) where R : RuleInstance
    {
        inst = null!;
        if (!TryFindTag(tag, tagDesc, out var nodeId) || !TAST.TryGetApplyRule(nodeId, out var rinst))
            return false;

        Debug.Assert(rinst is R);
        inst = (R)rinst;
        return true;
    }
    public R ResolveTag<R>(string tag, string tagDesc) where R : RuleInstance
    {
        if (!TAST.TryGetApplyRule(FindTag(tag, tagDesc), out var inst))
            throw new Exception($"TAG INSTANCE NOT FOUND: tag={tag}, tagDesc={tagDesc}");

        Debug.Assert(inst is R);
        return (R)inst;
    }

    //==== PERSISTENT TAGS ====
    private readonly Dictionary<TagKey, List<TagValue>> _persistTags = [];
    private void StorePTag(int nodeId, TagValue val)
    {
        TagKey key = new(_curPass, nodeId);
        if (!_persistTags.TryGetValue(key, out var list))
            list = _persistTags[key] = [];

        list.Add(val);
    }
    private void StoreOuterPTag(int nodeId, TagValue val)
    => StorePTag(TAST.NodeAt(nodeId).ParentId, val);

    internal readonly record struct TagKey
    (Pass Pass, int NodeId);
    internal readonly record struct TagValue
    (string Tag, string TagDesc);
}

//>>>> ATTRIBUTES <<<<
public interface AttrsView
{
    //HAS ATTRIBUTE
    public bool HasAttr(int nodeId, string attr);
    public bool HasAttr(int fileId, int nodeId, string attr);
    public bool HasAttr(FileNodeId nodeId, string attr)
    => HasAttr(nodeId.FileId, nodeId.NodeId, attr);
}
public interface Attrs : AttrsView
{
    //STORE ATTRIBUTE
    public void StoreAttr(string attr);
}
public partial class ParserProcess : Attrs
{
    //HAS ATTRIBUTE
    public bool HasAttr(int nodeId, string attr)
    => TAST.HasAttr(nodeId, attr);
    public bool HasAttr(int fileId, int nodeId, string attr)
    => Project.Files[fileId].TAST.HasAttr(nodeId, attr);

    //STORE ATTRIBUTE
    public void StoreAttr(string attr)
    => TAST.StoreAttr(RuleInst!.NodeId, attr);
}