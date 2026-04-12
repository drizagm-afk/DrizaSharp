using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Default.Parser;
using DrzSharp.Compiler.Rules.Lexer;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Model;

//TAST: Abstract Stratified Token Tree
//==================
//      Tokens
//==================
public sealed partial class TAST(SourceText source)
{
    private readonly SourceText _source = source;
    public void BuildFlatTAST()
    {
        if (_nodeCount <= 0) NewNode(0, _tokenCount, 0);
    }

    //TOKENS
    private Token[] _tokens = new Token[128];
    private int _tokenCount = 0;

    public Span<Token> Tokens => _tokens;
    public int TokenCount => _tokenCount;

    private int AddTokenItem(Token token)
    {
        var id = _tokenCount++;
        if (_tokens.Length <= _tokenCount)
            Array.Resize(ref _tokens, _tokens.Length * 2);

        _tokens[id] = token;
        return id;
    }
    public int NewToken(int type, int start, int length)
    => AddTokenItem(new(_tokenCount, type, start, length));
    public int NewToken(int type, int start, int length, string rephrase)
    {
        var id = NewToken(type, start, length);
        _tokenRephrases[id] = rephrase;

        return id;
    }

    public bool TryTokenAt(int tokenId, out Token token)
    {
        token = default;
        if (tokenId < 0 || tokenId >= _tokenCount)
            return false;

        token = _tokens[tokenId];
        return true;
    }
    public bool HasTokenAt(int tokenId) => TryTokenAt(tokenId, out _);
    public Token TokenAt(int tokenId)
    {
        if (!TryTokenAt(tokenId, out var token))
            throw new Exception($"TOKEN NOT FOUND: ID={tokenId}");

        return token;
    }

    private readonly Dictionary<int, string> _tokenRephrases = [];
    public void Rephrase(int tokenId, string rephrase)
    => _tokenRephrases[tokenId] = rephrase;
    public ReadOnlySpan<char> GetTextSpan(int tokenId)
    {
        var token = TokenAt(tokenId);
        if (token.IsNull)
            throw new Exception("YOU CANNOT CONVERT A NULL TOKEN INTO A CHAR SPAN");

        if (_tokenRephrases.TryGetValue(tokenId, out var val))
            return val.AsSpan();

        return _source.AsSpan(token.Start, token.Length);
    }
    public string GetText(int tokenId)
    {
        var token = TokenAt(tokenId);
        if (token.IsNull)
            throw new Exception("YOU CANNOT CONVERT A NULL TOKEN INTO A STRING");

        if (_tokenRephrases.TryGetValue(tokenId, out var val))
            return val;

        return _source.Slice(token.Start, token.Length);
    }
}

//===== TOKEN =====
public readonly struct Token(int id, int type, int start, int length)
{
    public readonly int Id = id;
    public readonly int Type = type;
    public readonly int Start = start;
    public readonly int Length = length;

    public bool IsNull => Type == Tokens.NULL_ID;
}

//=================
//      Nodes
//=================
public sealed partial class TAST
{
    //===== NODES =====
    //NODE
    private TASTNode[] _nodes = new TASTNode[128];
    private TASTInfo[] _nodeInfos = new TASTInfo[128];
    private int _nodeCount = 0;

    public const int RootId = 0;
    public ref readonly TASTNode Root
    {
        get
        {
            Debug.Assert(_nodeCount > 0);
            return ref _nodes[RootId];
        }
    }

    private int AddNodeItem(TASTNode node, TASTInfo info)
    {
        var id = _nodeCount++;
        if (_nodes.Length <= _nodeCount)
        {
            Array.Resize(ref _nodes, _nodes.Length * 2);
            Array.Resize(ref _nodeInfos, _nodeInfos.Length * 2);
        }

        _nodes[id] = node;
        _nodeInfos[id] = info;
        return id;
    }
    private int NewNode(
        int relStart, int relLength, int start, int? length = null,
        int firstChildId = -1, int nextSiblingId = -1, int parentId = -1, TASTInfo info = default
    )
    {
        var count = _nodeCount;
        TASTNode node = new(
            count, relStart, relLength, start, length ?? relLength,
            firstChildId, nextSiblingId, parentId
        );
        AddNodeItem(node, info);

        return count;
    }

    public bool HasNodeAt(int nodeId)
    => nodeId < 0 || nodeId >= _nodeCount;
    public bool TryNodeAt(int nodeId, out TASTNode node)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
        {
            node = default;
            return false;
        }
        node = _nodes[nodeId];
        return true;
    }
    public ref readonly TASTNode NodeAt(int nodeId)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
            throw new Exception($"Node not found in TAST: node={nodeId}");

        return ref _nodes[nodeId];
    }

    public void Update(
        int nodeId, int? relStart = null, int? relLength = null, int? start = null, int? length = null,
        int? firstChildId = null, int? nextSiblingId = null, int? parentId = null
    )
    => Update(NodeAt(nodeId), relStart, relLength, start, length, firstChildId, nextSiblingId, parentId);
    private void Update(
        in TASTNode node, int? relStart = null, int? relLength = null, int? start = null, int? length = null,
        int? firstChildId = null, int? nextSiblingId = null, int? parentId = null
    )
    {
        _nodes[node.Id] = new(
            node.Id, relStart ?? node.RelStart, relLength ?? node.RelLength, start ?? node.Start, length ?? node.Length,
            firstChildId ?? node.FirstChildId, nextSiblingId ?? node.NextSiblingId, parentId ?? node.ParentId
        );
    }

    public int Nest(int nodeId, int start, int length, TASTInfo info)
    {
        ref readonly var node = ref NodeAt(nodeId);
        Debug.Assert(0 <= start && start + length <= node.Length);

        int nestId = NewNode(start, length, node.Start + start, parentId: nodeId, info: info);
        int prevId = -1;
        int nextId = -1;
        int firstCId = -1;
        int lastCId = -1;

        int id = node.FirstChildId;
        while (id >= 0)
        {
            ref readonly var child = ref NodeAt(id);
            if (child.RelStart < start)
            {
                Debug.Assert(child.RelStart + child.RelLength <= start);
                prevId = id;
            }
            else if (child.RelStart < start + length)
            {
                Debug.Assert(child.RelStart + child.RelLength <= start + length);
                if (firstCId < 0) firstCId = id;
                lastCId = id;
                Update(child, relStart: child.RelStart - start, parentId: nestId);
            }
            else
            {
                nextId = id;
                break;
            }

            id = child.NextSiblingId;
        }
        return NewNest(nestId, nodeId, prevId, nextId, firstCId, lastCId);
    }
    private int NewNest
    (int nestId, int nodeId, int prevId, int nextId, int firstChildId, int lastChildId)
    {
        Update(nestId, nextSiblingId: nextId, firstChildId: firstChildId);
        if (prevId >= 0) Update(prevId, nextSiblingId: nestId);
        else Update(nodeId, firstChildId: nestId);

        if (lastChildId >= 0) Update(lastChildId, nextSiblingId: -1);

        return nestId;
    }

    public void Rewrite(int nodeId, Slice tokenSlice, ImmutableArray<int> tokenNodes)
    {
        Update(nodeId, start: tokenSlice.Start, length: tokenSlice.Length, firstChildId: FirstId(tokenNodes));

        Fill(nodeId, tokenSlice, tokenNodes);
    }

    public int Append(int nodeId, Slice tokenSlice, ImmutableArray<int> tokenNodes)
    {
        ref readonly var node = ref NodeAt(nodeId);

        int appendId = NewNode(
            -1, -1, tokenSlice.Start, tokenSlice.Length, FirstId(tokenNodes),
            parentId: nodeId, info: new(InfoAt(nodeId).RealmId)
        );

        if (node.FirstChildId < 0)
            Update(node.Id, firstChildId: appendId);
        else
        {
            ref readonly var child = ref NodeAt(node.FirstChildId);
            while (child.NextSiblingId >= 0)
                child = ref NodeAt(child.NextSiblingId);

            Update(child.Id, nextSiblingId: appendId);
        }

        Fill(appendId, tokenSlice, tokenNodes);
        return appendId;
    }

    private static int FirstId(ImmutableArray<int> tokenNodes)
    => tokenNodes.Length > 0 ? tokenNodes[0] : -1;
    private void Fill(int nodeId, Slice tokenSlice, ImmutableArray<int> tokenNodes)
    {
        int tokenNodeId = FirstId(tokenNodes);

        int i = 0;
        for (int t = 0; t < tokenSlice.Length; t++)
        {
            if (!HasTokenAt(t + tokenSlice.Start))
            {
                Debug.Assert(i < tokenNodes.Length);
                int nextId = ++i < tokenNodes.Length ? tokenNodes[i] : -1;
                Update(tokenNodeId, relStart: t, relLength: 1, nextSiblingId: nextId, parentId: nodeId);

                tokenNodeId = nextId;
            }
        }
    }

    //NODE INFOS
    public ref readonly TASTInfo InfoAt(int nodeId)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
            throw new Exception($"Node Info not found in TAST: node={nodeId}");

        return ref _nodeInfos[nodeId];
    }
    public void UpdateInfo(int nodeId, int? realmId = null, bool? isScoped = null, bool? isLinear = null)
    {
        ref readonly var info = ref InfoAt(nodeId);
        _nodeInfos[nodeId] = new(realmId ?? info.RealmId, isScoped ?? info.IsScoped, isLinear ?? info.IsLinear);
    }

    //===== NODE CURSOR METHODS =====
    //TO OFFSET
    public int SeekToOffset(in TASTNode node, int offset, out bool childExists, out TASTNode child)
    {
        childExists = TryNodeAt(node.FirstChildId, out child);

        while (childExists && child.RelStart < offset)
        {
            if (child.RelStart + child.RelLength > offset)
                throw new Exception($"ILLEGAL OFFSET: offset={offset}, relStart={child.RelStart}, relLength={child.RelLength}");

            childExists = TryNodeAt(child.NextSiblingId, out child);
        }
        return offset;
    }

    //TOKEN AT NODE
    public bool FindTokenAtNode(int nodeId, int offset, int start, out int tokenId)
    {
        tokenId = -1;
        ref readonly var node = ref NodeAt(nodeId);

        //NON-REWRITE TOKEN LOOKUP
        if (!InfoAt(nodeId).IsLinear)
        {
            int rel = offset + start;
            if (rel >= node.Length)
                return false;

            tokenId = node.Start + rel;
            return true;
        }

        //REWRITE TOKEN LOOKUP
        int i = SeekToOffset(node, offset, out var childExists, out var child);
        while (i < node.Length)
        {
            if (childExists && child.RelStart == i)
            {
                if (start < child.Length)
                {
                    tokenId = child.Start + start;
                    return true;
                }

                start -= child.Length;
                i += child.RelLength;
                childExists = TryNodeAt(child.NextSiblingId, out child);
                continue;
            }
            if (start == 0)
            {
                tokenId = node.Start + i;
                return true;
            }

            start--;
            i++;
        }
        return false;
    }

    public bool TryTokenAtNode(int nodeId, int offset, int start, out Token token)
    {
        token = default;
        if (FindTokenAtNode(nodeId, offset, start, out var tokenId))
            return TryTokenAt(tokenId, out token);

        return false;
    }
    public bool HasTokenAtNode(int nodeId, int offset, int start)
    => FindTokenAtNode(nodeId, offset, start, out var tokenId) && TryTokenAt(tokenId, out _);
    public Token TokenAtNode(int nodeId, int offset, int start)
    {
        if (!TryTokenAtNode(nodeId, offset, start, out var token))
            throw new Exception($"TOKEN NOT FOUND IN NODE: node={nodeId}, offset={offset}, start={start}");

        return token;
    }

    public bool TryTokenAtNode(TokenSpan span, out Token token)
    => TryTokenAtNode(span.NodeId, span.Offset, span.Start, out token);
    public bool HasTokenAtNode(TokenSpan span)
    => HasTokenAtNode(span.NodeId, span.Offset, span.Start);
    public Token TokenAtNode(TokenSpan span)
    => TokenAtNode(span.NodeId, span.Offset, span.Start);

    //NEST
    public bool TryGetNest(TokenSpan span, out int nestId)
    => TryGetNest(span.NodeId, span.Offset, span.Start, out nestId);
    public bool TryGetNest(int nodeId, int offset, int start, out int nestId)
    {
        nestId = -1;
        ref readonly var node = ref NodeAt(nodeId);

        int i = SeekToOffset(node, offset, out var childExists, out var child);
        while (i < node.Length)
        {
            if (!childExists)
                return false;
            if (child.RelStart == i)
            {
                if (start == 0)
                {
                    nestId = child.Id;
                    return true;
                }
                i += child.RelLength;
                start -= child.Length;
                childExists = TryNodeAt(child.NextSiblingId, out child);
                continue;
            }
            if (start <= 0) break;
            i++;
            start--;
        }
        return false;
    }

    //FLAT NODE
    public Slice ToFlatSlice(TokenSpan span)
    => ToFlatSlice(span.NodeId, span.Offset, span.Start, span.Length);
    public Slice ToFlatSlice(int nodeId, int offset, int start, int length)
    {
        ref readonly var node = ref NodeAt(nodeId);

        bool childExists;
        TASTNode child;
        int advance(in TASTNode node, int i, int count)
        {
            while (i < node.Length && count > 0)
            {
                if (childExists && child.RelStart == i)
                {
                    i += child.RelLength;
                    count -= child.Length;
                    childExists = TryNodeAt(child.NextSiblingId, out child);

                    continue;
                }
                i++;
                count--;
            }
            if (count != 0)
                throw new Exception($"ILLEGAL SPAN: node={nodeId}, nodeLength={node.Length}, offset={offset}, start={start}, length={length}");

            return i;
        }

        int flatStart = SeekToOffset(node, offset, out childExists, out child);
        flatStart = advance(node, flatStart, start);

        int flatEnd = flatStart;
        flatEnd = advance(node, flatEnd, length);

        return new(flatStart, flatEnd - flatStart);
    }

    public void UpdateLinearity(int nodeId)
    {
        ref readonly var node = ref NodeAt(nodeId);
        for (int i = 0; i < node.Length; i++)
        {
            if (TokenAt(i + node.Start).IsNull)
            {
                UpdateInfo(nodeId, isLinear: false);
                return;
            }
        }
    }

    //SOURCE SLICE
    public SourceSlice SourceSlice(int nodeId)
    => SourceSlice(NodeAt(nodeId));
    public SourceSlice SourceSlice(in TASTNode node)
    {
        if (node.Length <= 0)
            return default;

        var start = TokenAt(node.Start);
        var end = TokenAt(node.Start + node.Length - 1);
        return new(
            start.Start, end.Start + end.Length - start.Start
        );
    }
}

//===== NODES =====
public readonly struct TASTNode(
    int id, int relStart, int relLength, int start, int length,
    int firstChildId, int nextSiblingId, int parentId
)
{
    public readonly int Id = id;
    public readonly int RelStart = relStart;
    public readonly int RelLength = relLength;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
    public readonly int ParentId = parentId;

    public bool IsFlat() => FirstChildId < 0;
}
public readonly struct TokenSpan
{
    //NodeId
    public readonly int NodeId;

    //Value skipped when TryGetTokenByNode, relative to the NodeId's RelLength
    //This is evaluated before the Start
    public readonly int Offset;

    //Value skipped when TryGetTokenByNode, unlike offset, this isn't relative to NodeId's RelLength
    //This refers to the real position, which is measured with recursion, as Rewritten Nodes can change content
    public readonly int Start;

    //Length of the Span
    public readonly int Length;

    public TokenSpan(int nodeId, int offset, int start, int length)
    {
        Debug.Assert(length != 0);

        NodeId = nodeId;
        Offset = offset;
        Start = start;
        Length = length;
    }

    public TokenSpan With
    (int? nodeId = null, int? offset = null, int? start = null, int? length = null)
    => new(nodeId ?? NodeId, offset ?? Offset, start ?? Start, length ?? Length);

    public TokenSpan Skip(int skip = 1)
    => new(NodeId, Offset, Start + skip, Length < 0 ? -1 : Math.Max(Length - skip, 0));

    public bool IsValid => Length != 0;
}

//===== NODE INFO =====
public readonly struct TASTInfo(int realmId, bool isScoped = false, bool isLinear = true)
{
    public readonly int RealmId = realmId;
    public readonly bool IsScoped = isScoped;
    public readonly bool IsLinear = isLinear;
}

//>>>> NODE ATTACHMENTS <<<<
public sealed partial class TAST
{
    //NODE INSTANCES
    private Dictionary<int, RuleInstance> _ruleAppliance = [];
    public void ApplyRule(int nodeId, RuleInstance instance)
    => _ruleAppliance[nodeId] = instance;

    public bool TryGetApplyRule(int nodeId, [NotNullWhen(true)] out RuleInstance? instance)
    => _ruleAppliance.TryGetValue(nodeId, out instance);
    public bool HasApplyRule(int nodeId)
    => _ruleAppliance.ContainsKey(nodeId);
    public RuleInstance GetApplyRule(int nodeId)
    => _ruleAppliance[nodeId];

    //NODE ATTRIBUTES
    private readonly HashSet<AttrKey> _attributes = [];
    public bool StoreAttr(int nodeId, string attr)
    => _attributes.Add(new(nodeId, attr));
    public bool HasAttr(int nodeId, string attr)
    => _attributes.Contains(new(nodeId, attr));
}

//===== NODE ATTRIBUTES =====
internal readonly record struct AttrKey
(int NodeId, string Attr);