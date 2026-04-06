using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface IVReadOnlyAssembly
{
    public int Id { get; }
    public string Name { get; }

    public IVReadOnlyNspace ReadGlobalNspace();

    //**NODES**
    //NODE AT
    public ref readonly VNode NodeAt(int nodeId);
    public VKind KindOf(int nodeId);
    public bool IsKind(int nodeId, VKind kind);

    //INFO AT
    public IVReadOnlyInfo ReadInfoAt(int nodeId);
    public T ReadInfoAt<T>(int nodeId) where T : IVReadOnlyInfo;
    public bool TryReadInfoAt<T>(int nodeId, out T rinfo) where T : IVReadOnlyInfo;

    //**VNSPACE**
    public bool TryReadNspace(int outerNspaceId, string nspaceName, out IVReadOnlyNspace rinfo);
}
public partial class VAssembly : IVReadOnlyAssembly
{
    public int Id { get; }
    public string Name => Definition.Name.Name;

    public const int GlobalNspaceId = 0;
    public IVReadOnlyNspace ReadGlobalNspace()
    => ReadInfoAt<VNspace>(GlobalNspaceId);
    public VNspace EditGlobalNspace()
    => EditInfoAt<VNspace>(GlobalNspaceId);

    internal AssemblyDefinition Definition = null!;
    internal AssemblyHash Hash = default;

    //**NODES**
    internal VAssembly(int id)
    {
        Id = id;
        NewNode(VKind.Nspace, new VNspace(""));
    }

    private VNode[] _nodes = new VNode[128];
    private VInfo[] _nodeInfos = new VInfo[128];
    private int _nodeCount = 0;
    public int NodeCount => _nodeCount;

    private int NewNode(VKind kind, VInfo info, int parentId = -1, int nextSiblingId = -1)
    {
        var id = info.Id = _nodeCount++;

        var len = _nodes.Length;

        if (_nodeCount >= len)
        {
            Array.Resize(ref _nodes, len * 2);
            Array.Resize(ref _nodeInfos, len * 2);
        }
        _nodes[id] = new(id, kind, parentId, -1, nextSiblingId);
        _nodeInfos[id] = info;
        return id;
    }
    private int AddNode(VKind kind, VInfo info, int parentId = 0)
    {
        ref readonly var parent = ref NodeAt(parentId);

        var id = NewNode(kind, info, parentId, parent.FirstChildId);

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
    private T Edit<T>(int nodeId, string md) where T : VInfo
    => Edit<T>(ReadInfoAt(nodeId), md);
    private static T Edit<T>(IVReadOnlyInfo rinfo, string md) where T : VInfo
    {
        if (rinfo is not T info)
            throw new Exception($"VIRTUAL NODE IS NOT {typeof(T).Name.ToUpper()[1..]}: {md}");

        return info;
    }
    private static bool TryEdit<T>(IVReadOnlyInfo rinfo, out T info) where T : VInfo
    {
        info = null!;
        if (rinfo is not T einfo)
            return false;

        info = einfo;
        return true;
    }

    public IVReadOnlyInfo ReadInfoAt(int nodeId)
    => _nodeInfos[nodeId];
    public T ReadInfoAt<T>(int nodeId) where T : IVReadOnlyInfo
    => (T)ReadInfoAt(nodeId);
    public bool TryReadInfoAt<T>(int nodeId, out T rinfo) where T : IVReadOnlyInfo
    {
        rinfo = default!;
        if (_nodeInfos[nodeId] is not T eInfo)
            return false;

        rinfo = eInfo;
        return true;
    }

    public VInfo EditInfoAt(int nodeId)
    => _nodeInfos[nodeId];
    public T EditInfoAt<T>(int nodeId) where T : VInfo
    => (T)_nodeInfos[nodeId];
    public bool TryEditInfoAt<T>(int nodeId, out T info) where T : VInfo
    {
        info = null!;
        if (_nodeInfos[nodeId] is not T eInfo)
            return false;

        info = eInfo;
        return true;
    }

    //**VNSPACE**
    public bool TryReadNspace(int outerNspaceId, string nspaceName, out IVReadOnlyNspace rinfo)
    {
        var outer = ReadInfoAt<VNspace>(outerNspaceId);
        if (outer.Nspaces.TryGetValue(nspaceName, out int nodeId))
        {
            rinfo = ReadInfoAt<VNspace>(nodeId);
            return true;
        }
        rinfo = null!;
        return false;
    }
    public bool TryEditNspace(int outerNspaceId, string nspaceName, out VNspace info)
    {
        info = null!;
        return TryReadNspace(outerNspaceId, nspaceName, out var rinfo) && TryEdit(rinfo, out info);
    }

    public VNspace EnsureNspace(int outerNspaceId, string nspaceName)
    {
        if (!TryReadNspace(outerNspaceId, nspaceName, out var rinfo))
        {
            var outer = Edit<VNspace>(outerNspaceId, $"outerId={outerNspaceId} nspaceName={nspaceName}");
            var info = new VNspace(nspaceName);

            outer.NspacesMut[nspaceName] = AddNode(VKind.Nspace, info, outer.Id);
            return info;
        }

        return Edit<VNspace>(rinfo, $"outerId={outerNspaceId} nspaceName={nspaceName}");
    }
}
public static class EmptyDict
{
    public static readonly IReadOnlyDictionary<string, List<int>> Names = new Dictionary<string, List<int>>(0);
    public static readonly IReadOnlyDictionary<GenericId, List<int>> GenericNames = new Dictionary<GenericId, List<int>>(0);

    //SPECIAL
    public static readonly IReadOnlyDictionary<GenericId, int> Types = new Dictionary<GenericId, int>(0);
    public static readonly IReadOnlyDictionary<string, int> Nspaces = new Dictionary<string, int>(0);
    public static readonly IReadOnlyCollection<int> Ctors = Array.Empty<int>();
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
public interface IVReadOnlyInfo
{
    public int Id { get; }
    public string Name { get; }

    //METADATA
    public bool IsCompilerGenerated { get; }
}
public abstract class VInfo : IVReadOnlyInfo
{
    public int Id { get; internal set; }
    public string Name { get; }

    internal VInfo(string name)
    { Name = name; }

    //METADATA
    public bool IsCompilerGenerated { get; internal set; }
}

//>>>> VNSPACE <<<<
public interface IVReadOnlyTypeContainer
{
    public IReadOnlyDictionary<GenericId, int> Types { get; }
}
public interface VTypeContainer
{
    public Dictionary<GenericId, int> TypesMut { get; }
    public IReadOnlyDictionary<GenericId, int> Types { get; }
}

public interface IVReadOnlyNspace : IVReadOnlyInfo, IVReadOnlyTypeContainer
{
    //SCHEME
    public IReadOnlyDictionary<string, int> Nspaces { get; }
}
public sealed class VNspace : VInfo, IVReadOnlyNspace, VTypeContainer
{
    internal VNspace(string name) : base(name) { }

    //SCHEME
    Dictionary<string, int>? _nspaces;
    public Dictionary<string, int> NspacesMut { get => _nspaces ??= []; }
    public IReadOnlyDictionary<string, int> Nspaces => _nspaces ?? EmptyDict.Nspaces;

    Dictionary<GenericId, int>? _types;
    public Dictionary<GenericId, int> TypesMut { get => _types ??= []; }
    public IReadOnlyDictionary<GenericId, int> Types => _types ?? EmptyDict.Types;
}

//===== GENERICS =====
public readonly record struct GenericId(string Name, int GenericArity)
{
    public GenericId(string Name) : this(Name, 0) { }
    public override string ToString() => Name + '`' + GenericArity;
}
public readonly struct VGenericParam
(string name, ImmutableArray<UType> constraints, bool hasParamlessCtor = false, bool isReferenceType = false, bool isValueType = false)
{
    public string Name => name;
    public ImmutableArray<UType> Constraints => constraints;
    public bool HasParamlessCtor => hasParamlessCtor;
    public bool IsReferenceType => isReferenceType;
    public bool IsValueType => isValueType;
}

public interface IReadGeneric
{
    public int GenericArity { get; }
    public ImmutableArray<VGenericParam> GenericParams { get; }
}
public interface IGeneric : IReadGeneric
{
    public new int GenericArity { get; }
    public new ImmutableArray<VGenericParam> GenericParams { get; set; }
}

//===========================
//      VIRTUAL MEMBERS
//===========================
public interface IVReadOnlyMember : IVReadOnlyInfo { }
public abstract class VMember : VInfo, IVReadOnlyMember
{ internal VMember(string name) : base(name) { } }

//===========================
//    VIRTUAL MEMBER MODS
//===========================
public enum VMemberVisibility
{ PUBLIC, ASSEMBLY, PRIVATE, FAMILY, FAMILY_OR_ASSEMBLY, FAMILY_AND_ASSEMBLY }

public interface IReadVisibility { public VMemberVisibility Visibility { get; } }
public interface IVisibility : IReadVisibility { public new VMemberVisibility Visibility { get; set; } }

public interface IReadStatic { public bool IsStatic { get; } }
public interface IStatic : IReadStatic { public new bool IsStatic { get; set; } }

public interface IReadAbstract { public bool IsAbstract { get; } }
public interface IAbstract : IReadAbstract { public new bool IsAbstract { get; set; } }

public interface IReadVirtual { public bool IsVirtual { get; } }
public interface IVirtual : IReadVirtual { public new bool IsVirtual { get; set; } }

public interface IReadSealed { public bool IsSealed { get; } }
public interface ISealed : IReadSealed { public new bool IsSealed { get; set; } }