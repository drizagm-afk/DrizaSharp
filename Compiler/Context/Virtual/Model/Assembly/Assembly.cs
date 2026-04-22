using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public int Id { get; }
    public string Name { get; }

    public const int GlobalNspaceId = VAssemblyEdit.GlobalNspaceId;
    public VNspace ReadGlobalNspace();

    //NODES
    public int NodeCount { get; }
    public ref readonly VNode NodeAt(int nodeId);

    public VKind KindOf(int nodeId);

    //INFOS
    public VInfo ReadAt(int nodeId);
    public T ReadAt<T>(int nodeId) where T : VInfo;
    public bool TryReadAt<T>(int nodeId, out T read) where T : VInfo;

    //===== VNSPACE =====
    public bool IsMember(int nodeId);

    public IEnumerable<VNspace> ReadNspaces(int outerId);
    public bool TryReadNspace(int outerId, string nspaceName, out VNspace read);
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

    public const int EntryPointId = 1;

    internal AssemblyDefinition Definition = null!;
    internal AssemblyHash Hash = default;

    //**NODES**
    internal VAssemblyEdit(int id)
    {
        Id = id;
        NewNode(VKind.Nspace, new VNspaceEdit());
    }

    private VNode[] _nodes = new VNode[128];
    private VInfoEdit[] _nodeInfos = new VInfoEdit[128];
    private int _nodeCount = 0;

    private int NewNode(VKind kind, VInfoEdit edit, int parentId = -1, int nextSiblingId = -1)
    {
        var id = edit.Id = _nodeCount++;
        edit.AssemblyId = Id;

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

    //NODES
    public int NodeCount => _nodeCount;
    public ref readonly VNode NodeAt(int nodeId)
    => ref _nodes[nodeId];

    public VKind KindOf(int nodeId)
    => NodeAt(nodeId).Kind;

    //INFOS
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
    => ReadAt<T>(nodeId);
    public bool TryEditAt<T>(int nodeId, out T edit) where T : VInfoEdit
    => TryReadAt(nodeId, out edit);

    private static T Edit<T>(VInfo read) where T : VInfoEdit => (T)read;
    private static bool Edit<T>(VInfo read, out T edit) where T : VInfoEdit
    {
        edit = (T)read;
        return true;
    }

    //===== VNSPACE STRUCTURE =====
    private IReadOnlyDictionary<GenName, int> ReadNspaceTypes(int nspaceId)
    => EditAt<VNspaceEdit>(nspaceId)._types ?? Empty.IdByGenName;
    private Dictionary<GenName, int> EditNspaceTypes(int nspaceId)
    => EditAt<VNspaceEdit>(nspaceId)._types ??= [];

    private IReadOnlyDictionary<string, int> ReadNestedNspaces(int nspaceId)
    => EditAt<VNspaceEdit>(nspaceId)._nspaces ?? Empty.IdByName;
    private Dictionary<string, int> EditNestedNspaces(int nspaceId)
    => EditAt<VNspaceEdit>(nspaceId)._nspaces ??= [];

    //===== VNSPACE =====
    public bool IsMember(int nodeId)
    => KindOf(nodeId) != VKind.Nspace;

    public IEnumerable<VNspace> ReadNspaces(int outerId)
    {
        if (KindOf(outerId) != VKind.Nspace)
            throw new Exception($"NON-NSPACES DON'T HAVE NESTED NSPACES: symbolId{outerId}");

        foreach ((_, var id) in ReadNestedNspaces(outerId))
            yield return ReadAt<VNspace>(id);
    }
    public bool TryReadNspace(int outerId, string nspaceName, out VNspace read)
    {
        if (ReadNestedNspaces(outerId).TryGetValue(nspaceName, out int nodeId))
        {
            read = ReadAt<VNspace>(nodeId);
            return true;
        }
        read = null!;
        return false;
    }

    public IEnumerable<VNspaceEdit> EditNspaces(int outerId)
    {
        foreach (var nspace in ReadNspaces(outerId))
            yield return Edit<VNspaceEdit>(nspace);
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
            var outer = ReadAt<VNspace>(outerId);
            var edit = new VNspaceEdit(nspaceName, outer.FullName);

            EditNestedNspaces(outerId)[nspaceName] = AddNode(VKind.Nspace, edit, outerId);
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
    Object, Struct, Interface, //Delegate, Enum
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

    public int AssemblyId { get; }
    public GlobalId GlobalId { get; }

    //METADATA
    public bool IsCompilerGenerated { get; }
}
public abstract class VInfoEdit : VInfo
{
    public int Id { get; internal set; }
    public string Name { get; }

    public int AssemblyId { get; internal set; }
    public GlobalId GlobalId => new(AssemblyId, Id);

    internal VInfoEdit(string name)
    { Name = name; }

    //METADATA
    public bool IsCompilerGenerated { get; internal set; }
}

//>>>> NSPACE <<<<
public interface VNspace : VInfo
{
    public string FullName { get; }
}
public sealed class VNspaceEdit : VInfoEdit, VNspace
{
    public string FullName { get; }

    internal VNspaceEdit(string name = "", string? outerName = null) : base(name)
    {
        if (outerName is not null && outerName.Length > 0)
            FullName = $"{outerName}.{name}";
        else
            FullName = name;
    }

    //STRUCTURE
    internal Dictionary<GenName, int>? _types;
    internal Dictionary<string, int>? _nspaces;
}

//===== GENERICS =====
public readonly record struct GenName(string Name, int GenericArity)
{
    public GenName(string Name) : this(Name, 0) { }
    public override string ToString() => Name + '`' + GenericArity;
}
public interface VGenParam
{
    public string Name { get; }

    public VCollection<UType> Constraints { get; }
    public bool HasParamlessCtor { get; }
    public bool IsReferenceType { get; }
    public bool IsValueType { get; }
}
public sealed class VGenParamEdit : VGenParam
{
    public string Name { get; }
    internal VGenParamEdit(string name)
    { Name = name; }

    VCollectionEdit<UType>? _constraints;
    public VCollectionEdit<UType> ConstraintsMut { get => _constraints ??= []; }
    public VCollection<UType> Constraints => _constraints ?? Empty.UsageList;

    public bool HasParamlessCtor { get; set; }
    public bool IsReferenceType { get; set; }
    public bool IsValueType { get; set; }
}

public interface IGeneric
{
    public int GenericArity { get; }
    public VCollection<VGenParam> GenericParams { get; }
}
public interface IGenericEdit : IGeneric
{
    public VCollectionEdit<VGenParamEdit> GenericParamsMut { get; }
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

public enum VTypeLayout
{ AUTO, SEQUENTIAL, EXPLICIT }
public interface ILayout { public VTypeLayout Layout { get; } }
public interface ILayoutEdit : ILayout { public new VTypeLayout Layout { get; set; } }

public interface IStatic { public bool IsStatic { get; } }
public interface IStaticEdit : IStatic { public new bool IsStatic { get; set; } }

public interface IAbstract { public bool IsAbstract { get; } }
public interface IAbstractEdit : IAbstract { public new bool IsAbstract { get; set; } }

public interface ISealed { public bool IsSealed { get; } }
public interface ISealedEdit : ISealed { public new bool IsSealed { get; set; } }

public interface IVirtual { public bool IsVirtual { get; } }
public interface IVirtualEdit : IVirtual { public new bool IsVirtual { get; set; } }

public interface IFinal { public bool IsFinal { get; } }
public interface IFinalEdit : IFinal { public new bool IsFinal { get; set; } }