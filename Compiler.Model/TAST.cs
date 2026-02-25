using System.Diagnostics;
using DrzSharp.Compiler.Text;

namespace DrzSharp.Compiler.Model;

//TAST: Abstract Stratified Token Tree
public sealed class TAST(SourceSpan source)
{
    private readonly SourceSpan _source = source;
    public void BuildFlatTAST()
    {
        if (_nodeCount <= 0) NewNode(default, 0, _tokenCount, 0);
    }

    //**TOKENS**
    private Token[] _tokens = new Token[128];
    private int _tokenCount = 0;
    public int TokenCount => _tokenCount;

    private int AddTokenItem(Token token)
    {
        var id = _tokenCount++;
        if (_tokens.Length <= _tokenCount)
            Array.Resize(ref _tokens, _tokens.Length * 2);

        _tokens[id] = token;
        return id;
    }
    public int NewToken(byte type, int start, int length)
    => AddTokenItem(new(_tokenCount, type, start, length));
    public int NewToken(byte type, int start, int length, string rephrase)
    {
        var id = NewToken(type, start, length);
        _tokenRephases[id] = rephrase;

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

    private readonly Dictionary<int, string> _tokenRephases = [];
    public void Rephrase(int tokenId, string rephrase)
    => _tokenRephases[tokenId] = rephrase;
    public ReadOnlySpan<char> GetTextSpan(int tokenId)
    {
        var token = TokenAt(tokenId);
        if (token.IsNull)
            throw new Exception("YOU CANNOT CONVERT A NULL TOKEN INTO A CHAR SPAN");

        if (_tokenRephases.TryGetValue(tokenId, out var val))
            return val.AsSpan();

        return _source.AsSpan(token.Start, token.Length);
    }
    public string GetText(int tokenId)
    {
        var token = TokenAt(tokenId);
        if (token.IsNull)
            throw new Exception("YOU CANNOT CONVERT A NULL TOKEN INTO A STRING");

        if (_tokenRephases.TryGetValue(tokenId, out var val))
            return val;

        return _source.Slice(token.Start, token.Length);
    }

    //**NODES**
    internal Dictionary<int, EmitId> _outNodes = [];

    private TASTNode[] _nodes = new TASTNode[128];
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

    private int AddNodeItem(TASTNode node)
    {
        var id = _nodeCount++;
        if (_nodes.Length <= _nodeCount)
            Array.Resize(ref _nodes, _nodes.Length * 2);

        _nodes[id] = node;
        return id;
    }
    private int NewNode(
        TASTArgs args, int relStart, int relLength, int start, int? length = null,
        int firstChildId = -1, int nextSiblingId = -1, int parentId = -1
    )
    {
        var count = _nodeCount;
        TASTNode node = new(
            count, args,
            relStart, relLength, start, length ?? relLength,
            firstChildId, nextSiblingId, parentId
        );
        AddNodeItem(node);

        return count;
    }

    public ref readonly TASTNode NodeAt(int nodeId)
    {
        Debug.Assert(nodeId >= 0 || nodeId < _nodeCount, $"Node not found in TASI: node={nodeId}");
        return ref _nodes[nodeId];
    }
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

    public void Update(
        int nodeId, SchemeTASTArgs args = new(),
        int? relStart = null, int? relLength = null, int? start = null, int? length = null,
        int? firstChildId = null, int? nextSiblingId = null, int? parentId = null
    )
    => Update(NodeAt(nodeId), args, relStart, relLength, start, length, firstChildId, nextSiblingId, parentId);
    private void Update(
        in TASTNode node, SchemeTASTArgs args = new(),
        int? relStart = null, int? relLength = null, int? start = null, int? length = null,
        int? firstChildId = null, int? nextSiblingId = null, int? parentId = null
    )
    {
        _nodes[node.Id] = new(
            node.Id, args.Merge(node.Args),
            relStart ?? node.RelStart, relLength ?? node.RelLength, start ?? node.Start, length ?? node.Length,
            firstChildId ?? node.FirstChildId, nextSiblingId ?? node.NextSiblingId, parentId ?? node.ParentId
        );
    }

    public int Nest(int nodeId, int start, int length, SchemeTASTArgs args)
    {
        ref readonly var node = ref NodeAt(nodeId);
        Debug.Assert(0 <= start && start + length <= node.Length);

        int nestId = NewNode(args.Merge(node.Args), start, length, node.Start + start, parentId: nodeId);
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

    public void Rewrite(int nodeId, Slice tokenSlice, int[] fillNodes)
    {
        int fillId = fillNodes.Length > 0 ? fillNodes[0] : -1;
        Update(nodeId, start: tokenSlice.Start, length: tokenSlice.Length, firstChildId: fillId);

        int i = 0;
        for (int t = 0; t < tokenSlice.Length; t++)
        {
            if (!HasTokenAt(t + tokenSlice.Start))
            {
                Debug.Assert(i < fillNodes.Length);
                int nextId = ++i < fillNodes.Length ? fillNodes[i] : -1;
                Update(fillId, relStart: t, relLength: 1, nextSiblingId: nextId, parentId: nodeId);

                fillId = nextId;
            }
        }
    }

    public void Children(Action<int> action)
    => Children(RootId, action);
    public void Children(int nodeId, Action<int> action)
    {
        ref readonly var node = ref NodeAt(nodeId);
        var id = node.FirstChildId;
        while (id >= 0)
        {
            action(id);
            id = NodeAt(id).NextSiblingId;
        }
    }

    //NODE SOURCE COVERAGE
    public Slice SourceSlice(int nodeId)
    => SourceSlice(NodeAt(nodeId));
    public Slice SourceSlice(in TASTNode node)
    {
        if (node.Length <= 0)
            return default;

        var start = TokenAt(node.Start);
        var end = TokenAt(node.Start + node.Length - 1);
        return new(
            start.Start, end.Start + end.Length - start.Start
        );
    }

    //TOKEN AT NODE
    private bool FindTokenAtNode(int nodeId, int offset, int remOrder, out int tokenId)
    => FindTokenAtNode(NodeAt(nodeId), offset, ref remOrder, out tokenId);
    private bool FindTokenAtNode(in TASTNode node, int offset, ref int remOrder, out int tokenId)
    {
        if (node.FirstChildId < 0)
            return FindTokenAtFlatNode(node, offset, ref remOrder, out tokenId);
        else
            return FindTokenAtDeepNode(node, offset, ref remOrder, out tokenId);
    }
    private static bool FindTokenAtFlatNode(in TASTNode node, int offset, ref int remOrder, out int tokenId)
    {
        tokenId = default;
        if (node.Length < offset + remOrder)
        {
            remOrder -= node.Length - offset;
            return false;
        }

        tokenId = node.Start + offset + remOrder;
        return true;
    }
    private bool FindTokenAtDeepNode(in TASTNode node, int offset, ref int remOrder, out int tokenId)
    {
        tokenId = default;
        var i = SkipOffset(node, offset, out var childExists, out var child);
        while (i < node.Length)
        {
            if (childExists && child.RelStart == i)
            {
                if (FindTokenAtNode(child, 0, ref remOrder, out tokenId) && remOrder <= 0)
                    return true;

                i += child.RelLength;
                childExists = TryNodeAt(child.NextSiblingId, out child);
                continue;
            }

            if (remOrder <= 0)
            {
                tokenId = node.Start + i;
                return true;
            }
            remOrder--;
            i++;
        }
        return false;
    }
    public int SkipOffset(in TASTNode node, int offset, out bool childExists, out TASTNode child)
    {
        childExists = TryNodeAt(node.FirstChildId, out child);
        while (childExists && child.RelStart < offset)
        {
            Debug.Assert(child.RelStart + child.RelLength <= offset);
            childExists = TryNodeAt(child.NextSiblingId, out child);
        }

        return offset;
    }

    //TOKEN AT NODE
    public bool TryTokenAtNode(int nodeId, int offset, int order, out Token token)
    {
        token = default;
        if (FindTokenAtNode(nodeId, offset, order, out var tokenId))
            return TryTokenAt(tokenId, out token);

        return false;
    }
    public bool HasTokenAtNode(int nodeId, int offset, int order)
    => FindTokenAtNode(nodeId, offset, order, out var tokenId) && TryTokenAt(tokenId, out _);

    public Token TokenAtNode(int nodeId, int offset, int order)
    {
        if (!FindTokenAtNode(nodeId, offset, order, out var tokenId))
            throw new Exception($"TOKEN NOT FOUND AT: NODE={nodeId}, ORDER={order}");
        if (!TryTokenAt(tokenId, out var token))
            throw new Exception($"UNTRACKED NULL TOKEN AT: NODE={nodeId}, ORDER={order}");

        return token;
    }
}

//===== TOKENS =====
public readonly struct Token(int id, byte type, int start, int length)
{
    public const byte NULL = 0;
    public const byte NEWLINE = 1;

    public readonly int Id = id;
    public readonly byte Type = type;
    public readonly int Start = start;
    public readonly int Length = length;

    public bool IsNull => Type == NULL;
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

//===== NODES =====
public readonly struct TASTNode(
    int id, TASTArgs args, int relStart, int relLength, int start, int length,
    int firstChildId, int nextSiblingId, int parentId
)
{
    public readonly int Id = id;
    public readonly TASTArgs Args = args;
    public readonly int RelStart = relStart;
    public readonly int RelLength = relLength;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
    public readonly int ParentId = parentId;

    public bool IsFlat() => FirstChildId < 0;
}
public readonly struct TASTArgs
(byte outCode, byte realmCode, bool isScoped)
{
    public readonly byte OutCode = outCode;
    public readonly byte RealmCode = realmCode;
    public readonly bool IsScoped = isScoped;

    public TASTArgs With(byte? outCode = 0, byte? realmCode = null, bool? isScoped = null)
    => new(outCode ?? OutCode, realmCode ?? RealmCode, isScoped ?? IsScoped);
    public TASTArgs With(SchemeTASTArgs scheme) => scheme.Merge(this);
    public SchemeTASTArgs AsScheme() => new(OutCode, RealmCode, IsScoped);

    public RealmId RealmId => new(OutCode, RealmCode);
}
public readonly struct SchemeTASTArgs
{
    private const byte OutArg = 0b10;
    private const byte RealmArg = 0b1;
    private const byte IsScopedArg = 0b100;

    public readonly byte OutCode;
    public readonly byte RealmCode;
    public readonly bool IsScoped;
    public readonly byte Args;

    public SchemeTASTArgs(byte? outCode = null, byte? realmCode = null, bool? isScoped = null)
    {
        if (outCode is byte _out)
        {
            OutCode = _out;
            Args += OutArg;
        }
        if (realmCode is byte _realm)
        {
            RealmCode = _realm;
            Args += RealmArg;
        }
        if (isScoped is bool _isScoped)
        {
            IsScoped = _isScoped;
            Args += IsScopedArg;
        }
    }
    private bool HasArg(byte arg)
    => (Args & arg) == arg;
    public TASTArgs Merge(TASTArgs args)
    => new(
        HasArg(OutArg) ? OutCode : args.OutCode,
        HasArg(RealmArg) ? RealmCode : args.RealmCode,
        HasArg(IsScopedArg) ? IsScoped : args.IsScoped
    );
}
public readonly struct RealmId(byte phaseCode, byte realmCode)
{
    public readonly byte PhaseCode = phaseCode;
    public readonly byte RealmCode = realmCode;

    public bool Equals(RealmId other)
    => PhaseCode == other.PhaseCode && RealmCode == other.RealmCode;
}