using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public int Id { get; }
    public string Name { get; }

    public VNspace ReadGlobalNspace();

    //**NODES**
    public int NodeCount { get; }
    //NODE AT
    public ref readonly VNode NodeAt(int nodeId);
    public VKind KindOf(int nodeId);
    public bool IsKind(int nodeId, VKind kind);

    //INFO AT
    public VInfo ReadAt(int nodeId);
    public T ReadAt<T>(int nodeId) where T : VInfo;
    public bool TryReadAt<T>(int nodeId, out T read) where T : VInfo;

    //**VNSPACE**
    public bool TryReadNspace(int outerId, string name, out VNspace read);
}
public partial class VAssemblyEdit : VAssembly
{
    public int Id { get; }
    public string Name => Definition.Name.Name;

    public const int GlobalNspaceId = 0;
    public VNspace ReadGlobalNspace()
    => ReadAt<VNspace>(GlobalNspaceId);
    public VNspaceEdit EditGlobalNspace()
    => EditAt<VNspaceEdit>(GlobalNspaceId);

    internal AssemblyDefinition Definition = null!;
    internal AssemblyHash Hash = default;

    //**NODES**
    internal VAssemblyEdit(int id)
    {
        Id = id;
        NewNode(VKind.Nspace, new VNspaceEdit(""));
    }

    private VNode[] _nodes = new VNode[128];
    private VInfoEdit[] _nodeInfos = new VInfoEdit[128];
    private int _nodeCount = 0;
    public int NodeCount => _nodeCount;

    private int NewNode(VKind kind, VInfoEdit edit, int parentId = -1, int nextSiblingId = -1)
    {
        var id = edit.Id = _nodeCount++;

        var len = _nodes.Length;

        if (_nodeCount >= len)
        {
            Array.Resize(ref _nodes, len * 2);
            Array.Resize(ref _nodeInfos, len * 2);
        }
        _nodes[id] = new(id, kind, parentId, -1, nextSiblingId);
        _nodeInfos[id] = edit;
        return id;
    }
    private int AddNode(VKind kind, VInfoEdit edit, int parentId = 0)
    {
        ref readonly var parent = ref NodeAt(parentId);

        var id = NewNode(kind, edit, parentId, parent.FirstChildId);

        //UPDATING PARENT
        _nodes[parentId] = new(
            parent.Id, parent.Kind, parent.ParentId, id, parent.NextSiblingId
        );
        return id;
    }

    //NODE AT
    public ref readonly VNode NodeAt(int nodeId)
    => ref _nodes[nodeId];

    public VKind KindOf(int nodeId)
    => NodeAt(nodeId).Kind;
    public bool IsKind(int nodeId, VKind kind)
    => KindOf(nodeId) == kind;

    //INFO AT
    public VInfo ReadAt(int nodeId)
    => _nodeInfos[nodeId];
    public T ReadAt<T>(int nodeId) where T : VInfo
    => (T)ReadAt(nodeId);
    public bool TryReadAt<T>(int nodeId, out T read) where T : VInfo
    {
        read = default!;
        if (_nodeInfos[nodeId] is not T eInfo)
            return false;

        read = eInfo;
        return true;
    }

    public VInfoEdit EditAt(int nodeId)
    => _nodeInfos[nodeId];
    public T EditAt<T>(int nodeId) where T : VInfoEdit
    => (T)_nodeInfos[nodeId];
    public bool TryEditAt<T>(int nodeId, out T edit) where T : VInfoEdit
    {
        edit = default!;
        if (_nodeInfos[nodeId] is not T eInfo)
            return false;

        edit = eInfo;
        return true;
    }

    private static T Edit<T>(VInfo read) where T : VInfoEdit
    => (T)read;
    private static bool Edit<T>(VInfo read, out T edit) where T : VInfoEdit
    {
        edit = (T)read;
        return true;
    }

    //**VNSPACE**
    public bool TryReadNspace(int outerId, string nspaceName, out VNspace read)
    {
        var outer = ReadAt<VNspace>(outerId);
        if (outer.Nspaces.TryGetValue(nspaceName, out int nodeId))
        {
            read = ReadAt<VNspace>(nodeId);
            return true;
        }
        read = null!;
        return false;
    }
    public bool TryEditNspace(int outerId, string nspaceName, out VNspaceEdit edit)
    {
        edit = null!;
        return TryReadNspace(outerId, nspaceName, out var read) && Edit(read, out edit);
    }

    public VNspaceEdit EnsureNspace(int outerId, string nspaceName)
    {
        if (!TryReadNspace(outerId, nspaceName, out var read))
        {
            var outer = EditAt<VNspaceEdit>(outerId);
            var edit = new VNspaceEdit(nspaceName);

            outer.NspacesMut[nspaceName] = AddNode(VKind.Nspace, edit, outer.Id);
            return edit;
        }

        return Edit<VNspaceEdit>(read);
    }
}

//===== VIRTUAL NODES =====
public readonly struct VNode
(int id, VKind kind, int parentId, int firstChildId, int nextSiblingId)
{
    public readonly int Id = id;
    public readonly VKind Kind = kind;

    public readonly int ParentId = parentId;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
}
public enum VKind
{
    Nspace,
    Type, Interface, //Delegate, Enum
    Field, //Constant,
    Property, //Indexer,
    Method, Ctor, Accessor, //Operator, Converter, Dctor
}

//========================
//      VIRTUAL CORE
//========================
public interface VInfo
{
    public int Id { get; }
    public string Name { get; }

    //METADATA
    public bool IsCompilerGenerated { get; }
}
public abstract class VInfoEdit : VInfo
{
    public int Id { get; internal set; }
    public string Name { get; }

    internal VInfoEdit(string name)
    { Name = name; }

    //METADATA
    public bool IsCompilerGenerated { get; internal set; }
}

//>>>> TYPE CONTAINER <<<<
public interface VTypeContainer
{
    public IReadOnlyDictionary<GenName, int> Types { get; }
}
public interface VTypeContainerEdit : VTypeContainer
{
    public Dictionary<GenName, int> TypesMut { get; }
}

//>>>> NSPACE <<<<
public interface VNspace : VInfo, VTypeContainer
{
    //SCHEME
    public IReadOnlyDictionary<string, int> Nspaces { get; }
}
public sealed class VNspaceEdit : VInfoEdit, VNspace, VTypeContainerEdit
{
    internal VNspaceEdit(string name) : base(name) { }

    //SCHEME
    Dictionary<string, int>? _nspaces;
    public Dictionary<string, int> NspacesMut { get => _nspaces ??= []; }
    public IReadOnlyDictionary<string, int> Nspaces => _nspaces ?? Empty.IdByName;

    Dictionary<GenName, int>? _types;
    public Dictionary<GenName, int> TypesMut { get => _types ??= []; }
    public IReadOnlyDictionary<GenName, int> Types => _types ?? Empty.IdByGenName;
}

//===== GENERICS =====
public readonly record struct GenName(string Name, int GenericArity)
{
    public GenName(string Name) : this(Name, 0) { }
    public override string ToString() => Name + '`' + GenericArity;
}
public readonly struct VGenParam
(string name, ImmutableArray<UType> constraints, bool hasParamlessCtor = false, bool isReferenceType = false, bool isValueType = false)
{
    public string Name => name;
    public ImmutableArray<UType> Constraints => constraints;
    public bool HasParamlessCtor => hasParamlessCtor;
    public bool IsReferenceType => isReferenceType;
    public bool IsValueType => isValueType;
}

public interface IGeneric
{
    public int GenericArity { get; }
    public ImmutableArray<VGenParam> GenericParams { get; }
}
public interface IGenericEdit : IGeneric
{
    public new ImmutableArray<VGenParam> GenericParams { get; set; }
}

//===========================
//      VIRTUAL MEMBERS
//===========================
public interface VMember : VInfo { }
public abstract class VMemberEdit : VInfoEdit, VMember
{
    internal VMemberEdit(string name) : base(name) { }
}

//===========================
//    VIRTUAL MEMBER MODS
//===========================
public enum VMemberVisibility
{ PUBLIC, ASSEMBLY, PRIVATE, FAMILY, FAMILY_OR_ASSEMBLY, FAMILY_AND_ASSEMBLY }

public interface IVisibility { public VMemberVisibility Visibility { get; } }
public interface IVisibilityEdit : IVisibility { public new VMemberVisibility Visibility { get; set; } }

public interface IStatic { public bool IsStatic { get; } }
public interface IStaticEdit : IStatic { public new bool IsStatic { get; set; } }

public interface IAbstract { public bool IsAbstract { get; } }
public interface IAbstractEdit : IAbstract { public new bool IsAbstract { get; set; } }

public interface IVirtual { public bool IsVirtual { get; } }
public interface IVirtualEdit : IVirtual { public new bool IsVirtual { get; set; } }

public interface ISealed { public bool IsSealed { get; } }
public interface ISealedEdit : ISealed { public new bool IsSealed { get; set; } }