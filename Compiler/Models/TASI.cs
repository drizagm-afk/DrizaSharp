using System.Buffers.Binary;

namespace DrzSharp.Compiler.Model;

//TASI: Abstract Stratified Instruction Tree
//========================
//      Instructions
//========================
public sealed partial class TASI
{
    //**DATA**
    private byte[] _dataTable = new byte[128];
    private int _dataCount = 0;
    public int DataCount => _dataCount;

    private object[] _refTable = new object[128];
    private int _refCount = 0;

    public const byte BYTE_SIZE = 1;
    public const byte INT32_SIZE = 4;
    public const byte INT64_SIZE = 8;
    public const byte FLOAT32_SIZE = 4;
    public const byte FLOAT64_SIZE = 8;

    public const byte REF_SIZE = INT32_SIZE;
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
    public int WriteInt32(int val)
    {
        AddDataCap(INT32_SIZE, out var off);
        BinaryPrimitives.WriteInt32LittleEndian(
            _dataTable.AsSpan(off, INT32_SIZE), val
        );

        return off;
    }
    public int WriteInt64(long val)
    {
        AddDataCap(INT64_SIZE, out var off);
        BinaryPrimitives.WriteInt64LittleEndian(
            _dataTable.AsSpan(off, INT64_SIZE), val
        );

        return off;
    }
    public int WriteFloat32(float val)
    {
        AddDataCap(FLOAT32_SIZE, out var off);
        BinaryPrimitives.WriteSingleLittleEndian(
            _dataTable.AsSpan(off, FLOAT32_SIZE), val
        );

        return off;
    }
    public int WriteFloat64(double val)
    {
        AddDataCap(FLOAT64_SIZE, out var off);
        BinaryPrimitives.WriteDoubleLittleEndian(
            _dataTable.AsSpan(off, FLOAT64_SIZE), val
        );

        return off;
    }

    public int WriteObject(object val)
    {
        var off = _refCount++;
        if (_refTable.Length <= _refCount)
            Array.Resize(ref _refTable, _refTable.Length * 2);
        _refTable[off] = val;

        return WriteInt32(off);
    }
    public int WriteString(string val) => WriteObject(val);

    //READ
    public byte ReadByte(int offset)
    => _dataTable[offset];
    public int ReadInt32(int offset)
    => BinaryPrimitives.ReadInt32LittleEndian(_dataTable.AsSpan(offset, INT32_SIZE));
    public long ReadInt64(int offset)
    => BinaryPrimitives.ReadInt64LittleEndian(_dataTable.AsSpan(offset, INT64_SIZE));
    public float ReadFloat32(int offset)
    => BinaryPrimitives.ReadSingleLittleEndian(_dataTable.AsSpan(offset, FLOAT32_SIZE));
    public double ReadFloat64(int offset)
    => BinaryPrimitives.ReadDoubleLittleEndian(_dataTable.AsSpan(offset, FLOAT64_SIZE));

    public object ReadObject(int offset)
    => _refTable[ReadInt32(offset)];
    public string ReadString(int offset)
    => (string)ReadObject(offset);

    //**INSTRUCTIONS**
    private Instr[] _instructions = new Instr[128];
    private int _instCount = 0;
    public int InstructionCount => _instCount;
    public int NewInstruction(InstrType type, int start, int length, SourceSlice source = default)
    {
        var off = _instCount++;
        if (_instructions.Length <= _instCount)
            Array.Resize(ref _instructions, _instructions.Length * 2);

        _instructions[off] = new(type, start, length, source);
        return off;
    }
    public Instr InstructionAt(int instructionId)
    => _instructions[instructionId];
}

//===== INSTRUCTIONS =====
public readonly struct Instr(InstrType type, int start, int length, SourceSlice source)
{
    public readonly InstrType Type = type;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly SourceSlice Source = source;
}
public enum InstrType
{
    None,
    
    //CONSTANTS
    LdcInt32, LdcInt64, LdcFloat32, LdcFloat64, Ldstr, Ldnull,
    //STACK
    Dup, Pop,

    //>>>> MATH <<<<
    //COMPARISON
    Equal, GreaterThan, LessThan,
    //ARITHMETIC
    Add, Sub, Mul, Div, Rem,
    //BITWISE
    And, Or, Xor, Not, ShiftLeft, ShiftRight,

    //>>>> STORAGE <<<<
    //LOCALS
    DeclLocal, LoadLocal, StoreLocal,

    //>>>> FLOW <<<<
    Return,
    //BRANCHES
    Label, Br, BrTrue, BrFalse,

    //>>>> TEMPORAL <<<<
    EnterMethod, Print,
}

//=================
//      Nodes
//=================
public sealed partial class TASI
{
    //===== NODES =====
    //NODES
    private TASINode[] _nodes = new TASINode[128];
    private TASIInfo[] _nodeInfos = new TASIInfo[128];
    private int _nodeCount = 0;
    public int NodeCount => _nodeCount;

    public const byte RootId = 0;
    public ref readonly TASINode Root => ref _nodes[RootId];
    public TASI() => NewNode(-1, 0, 0, default);

    private int NewNode(int relIndex, int start, int length, TASIInfo info)
    {
        var id = _nodeCount++;
        if (_nodeCount >= _nodes.Length)
        {
            Array.Resize(ref _nodes, _nodes.Length * 2);
            Array.Resize(ref _nodeInfos, _nodeInfos.Length * 2);
        }

        _nodes[id] = new(id, relIndex, start, length, -1, -1);
        _nodeInfos[id] = info;
        return id;
    }

    public bool HasNodeAt(int nodeId)
    => nodeId < 0 || nodeId >= _nodeCount;
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
    public ref readonly TASINode NodeAt(int nodeId)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
            throw new Exception($"Node not found in TASI: node={nodeId}");

        return ref _nodes[nodeId];
    }

    private void Update(int nodeId, int? firstChildId = null, int? nextSiblingId = null)
    {
        ref readonly var node = ref NodeAt(nodeId);
        _nodes[node.Id] = new TASINode(
            node.Id, node.RelIndex, node.Start, node.Length,
            firstChildId ?? node.FirstChildId, nextSiblingId ?? node.NextSiblingId
        );
    }

    public int AddNode(int start, int length, TASIInfo info)
    => AddNode(0, 0, start, length, info);
    public int AddNode(int parentId, int index, int start, int length, TASIInfo info)
    {
        ref readonly var parent = ref NodeAt(parentId);
        int nestId = NewNode(index, start, length, info);

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

    //NODE INFO
    public ref readonly TASIInfo InfoAt(int nodeId)
    {
        if (nodeId < 0 || nodeId >= _nodeCount)
            throw new Exception($"Node Info not found in TASI: node={nodeId}");

        return ref _nodeInfos[nodeId];
    }
}

//===== NODES =====
public readonly struct TASINode(
    int id, int relIndex, int start, int length,
    int firstChildId, int nextSiblingId
)
{
    public readonly int Id = id;
    public readonly int RelIndex = relIndex;
    public readonly int Start = start;
    public readonly int Length = length;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
}

//===== NODE INFO =====
public readonly struct TASIInfo
(int sourceNodeId)
{
    public readonly int SourceNodeId = sourceNodeId;
}