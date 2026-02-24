using System.Buffers.Binary;
using System.Diagnostics;

namespace DrzSharp.Compiler.Model;

//TASI: Abstract Stratified Instruction Tree
public sealed class TASI
{
    //**DATA**
    private byte[] _dataTable = new byte[128];
    private int _dataCount = 0;
    private object[] _refTable = new object[128];
    private int _refCount = 0;

    public const byte BYTE_SIZE = 1;
    public const byte INT_SIZE = 4;
    public const byte REF_SIZE = 4;
    private void AddDataCap(int count, out int offset)
    {
        offset = _dataCount;
        _dataCount += count;
        if (_dataTable.Length <= _dataCount)
            Array.Resize(ref _dataTable, _dataTable.Length * 2);
    }

    //WRITE
    public int WriteByte(byte val)
    {
        AddDataCap(BYTE_SIZE, out var off);
        _dataTable[off] = val;

        return off;
    }
    public int WriteInt(int val)
    {
        AddDataCap(INT_SIZE, out var off);
        BinaryPrimitives.WriteInt32LittleEndian(
            _dataTable.AsSpan(off, INT_SIZE), val
        );

        return off;
    }

    public int WriteObject(object val)
    {
        var off = _refCount++;
        if (_refTable.Length <= _refCount)
            Array.Resize(ref _refTable, _refTable.Length * 2);
        _refTable[off] = val;

        return WriteInt(off);
    }
    public int WriteString(string val) => WriteObject(val);

    //READ
    public byte ReadByte(int offset)
    => _dataTable[offset];
    public int ReadInt(int offset)
    => BinaryPrimitives.ReadInt32LittleEndian(_dataTable.AsSpan(offset, INT_SIZE));

    public object ReadObject(int offset)
    => _refTable[ReadInt(offset)];
    public string ReadString(int offset)
    => (string)ReadObject(offset);

    //**INSTRUCTIONS**
    private Instruction[] _instructions = new Instruction[128];
    private int _instCount = 0;
    public int InstructionCount => _instCount;
    public int NewInstruction(Lowerer.RuleId ruleId, int start, int length, Slice source = new())
    {
        var off = _instCount++;
        if (_instructions.Length <= _instCount)
            Array.Resize(ref _instructions, _instructions.Length * 2);

        _instructions[off] = new(ruleId, start, length, source);
        return off;
    }
    public Instruction InstructionAt(int instructionId)
    => _instructions[instructionId];

    //**NODES**
    private TASINode[] _nodes = new TASINode[128];
    private int _nodeCount = 0;

    //ROOT
    public const byte RootId = 0;
    public ref readonly TASINode Root => ref _nodes[RootId];
    public TASI() => NewNode(-1, 0, 0);

    private int NewNode(int relIndex, int start, int length, Parser.RuleId source = new())
    {
        var id = _nodeCount++;
        if (_nodeCount >= _nodes.Length)
            Array.Resize(ref _nodes, _nodes.Length * 2);

        _nodes[id] = new(id, relIndex, start, length, -1, -1, source);
        return id;
    }

    public ref readonly TASINode NodeAt(int nodeId)
    {
        Debug.Assert(nodeId >= 0 && nodeId < _nodeCount, $"Node not found in TASI: node={nodeId}");
        return ref _nodes[nodeId];
    }
    public bool TryNodeAt(int nodeId, out TASINode node)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
        {
            node = default;
            return false;
        }
        node = _nodes[nodeId];
        return true;
    }

    private void Update(int nodeId, int? firstChildId = null, int? nextSiblingId = null)
    {
        ref readonly var node = ref NodeAt(nodeId);
        _nodes[node.Id] = new TASINode(
            node.Id, node.RelIndex, node.Start, node.Length,
            firstChildId ?? node.FirstChildId, nextSiblingId ?? node.NextSiblingId, 
            node.Source
        );
    }
    public int AddNode(int start, int length, Parser.RuleId source = new())
    => AddNode(0, 0, start, length, source);
    public int AddNode(int parentId, int index, int start, int length, Parser.RuleId source = new())
    {
        ref readonly var parent = ref NodeAt(parentId);
        int nestId = NewNode(index, start, length, source);

        var childId = parent.FirstChildId;
        (int prevId, int nextId) = (-1, childId);

        while (TryNodeAt(childId, out var child) && child.RelIndex > index)
        {
            prevId = childId;
            nextId = childId = child.NextSiblingId;
        }
        return NewChild(parentId, nestId, prevId, nextId);
    }
    private int NewChild
    (int parentId, int nestId, int prevId, int nextId)
    {
        Update(nestId, nextSiblingId: nextId);
        if (prevId >= 0) Update(prevId, nextSiblingId: nestId);
        else Update(parentId, firstChildId: nestId);

        return nestId;
    }
}

//===== INSTRUCTIONS =====
public readonly struct Instruction(Lowerer.RuleId ruleId, int start, int length, Slice source)
{
    public readonly Lowerer.RuleId RuleId = ruleId;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly Slice Source = source;
}

//===== NODES =====
public readonly struct TASINode(
    int id, int relIndex, int start, int length,
    int firstChildId, int nextSiblingId, Parser.RuleId source
)
{
    public readonly int Id = id;
    public readonly int RelIndex = relIndex;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
    public readonly Parser.RuleId Source = source;
}
public readonly struct EmitId(int parentId, int index)
{
    public readonly int ParentId = parentId;
    public readonly int Index = index;
    public readonly bool IsValid = true;
}