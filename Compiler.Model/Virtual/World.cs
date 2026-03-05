using System.Collections.Immutable;

namespace DrzSharp.Compiler.Model;

public partial class VirtualWorld
{
    //**NODES**
    public VirtualWorld() => NewNode(VKind.Root, new VRoot());
    internal const int RootId = 0;

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

    public VInfo InfoAt(int nodeId)
    => _nodeInfos[nodeId];
    public T InfoAt<T>(int nodeId) where T : VInfo
    => (T)InfoAt(nodeId);
    public bool TryInfoAt<T>(int nodeId, out T info) where T : VInfo
    {
        info = null!;

        var bInfo = InfoAt(nodeId);
        if (bInfo is not T eInfo)
            return false;

        info = eInfo;
        return true;
    }

    //**ENSURE**
    //ENSURE ASSEMBLY
    private readonly Dictionary<int, int> _assemblies = [];
    public IReadOnlyDictionary<int, int> Assemblies => _assemblies;

    public bool TryGetAssembly(int assemblyHash, out VAssembly info)
    {
        if (_assemblies.TryGetValue(assemblyHash, out int nodeId))
        {
            info = InfoAt<VAssembly>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VAssembly AddAssembly(int assemblyHash)
    {
        var info = new VAssembly(assemblyHash);
        _assemblies[assemblyHash]
        = AddNode(VKind.Assembly, info);

        return info;
    }
    public VAssembly EnsureAssembly(int assemblyHash)
    => TryGetAssembly(assemblyHash, out var info)
    ? info : AddAssembly(assemblyHash);

    //ENSURE NAMESPACE
    public bool TryGetNspace(VAssembly vAsm, string nspaceName, out VNspace info)
    {
        if (vAsm._nspaces.TryGetValue(nspaceName, out int nodeId))
        {
            info = InfoAt<VNspace>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VNspace AddNspace(VAssembly vAsm, string nspaceName)
    {
        var info = new VNspace(nspaceName);
        vAsm._nspaces[nspaceName]
        = AddNode(VKind.Nspace, info, vAsm.Id);

        return info;
    }
    public VNspace EnsureNspace(VAssembly vAsm, string nspaceName)
    => TryGetNspace(vAsm, nspaceName, out var info)
    ? info : AddNspace(vAsm, nspaceName);

    public bool TryGetNspace(VNspace vOuterNspace, string nspaceName, out VNspace info)
    {
        if (vOuterNspace._nestedNspaces.TryGetValue(nspaceName, out int nodeId))
        {
            info = InfoAt<VNspace>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VNspace AddNspace(VNspace vOuterNspace, string nspaceName)
    {
        var info = new VNspace(nspaceName);
        vOuterNspace._nestedNspaces[nspaceName]
        = AddNode(VKind.Nspace, info, vOuterNspace.Id);

        return info;
    }
    public VNspace EnsureNspace(VNspace vOuterNspace, string nspaceName)
    => TryGetNspace(vOuterNspace, nspaceName, out var info)
    ? info : AddNspace(vOuterNspace, nspaceName);

    //ENSURE TYPE
    public bool TryGetType
    (VNspace vNspace, string typeName, int genericArity, out VType info)
    {
        if (vNspace._types.TryGetValue(new(typeName, genericArity), out int nodeId))
        {
            info = InfoAt<VType>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VType AddType
    (VNspace vNspace, string typeName, int genericArity)
    {
        var info = new VType(typeName, genericArity);
        vNspace._types[new(typeName, genericArity)]
        = AddNode(VKind.Type, info, vNspace.Id);

        return info;
    }
    public VType EnsureType
    (VNspace vNspace, string typeName, int genericArity)
    => TryGetType(vNspace, typeName, genericArity, out var info)
    ? info : AddType(vNspace, typeName, genericArity);

    public bool TryGetType
    (VType vOuterType, string typeName, int genericArity, out VType info)
    {
        if (vOuterType._nestedTypes.TryGetValue(new(typeName, genericArity), out int nodeId))
        {
            info = InfoAt<VType>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VType AddType
    (VType vOuterType, string typeName, int genericArity)
    {
        var info = new VType(typeName, genericArity);
        vOuterType._nestedTypes[new(typeName, genericArity)]
        = AddNode(VKind.Type, info, vOuterType.Id);

        return info;
    }
    public VType EnsureType
    (VType vOuterType, string typeName, int genericArity)
    => TryGetType(vOuterType, typeName, genericArity, out var info)
    ? info : AddType(vOuterType, typeName, genericArity);

    //ENSURE FIELD
    public bool TryGetField(VType vType, string fieldName, out VField info)
    {
        if (vType._fields.TryGetValue(fieldName, out int nodeId))
        {
            info = InfoAt<VField>(nodeId);
            return true;
        }
        info = null!;
        return false;
    }
    public VField AddField(VType vType, string fieldName)
    {
        var info = new VField(fieldName);
        vType._fields[fieldName] = AddNode(VKind.Field, info, vType.Id);
        return info;
    }
    public VField EnsureField(VType vType, string fieldName)
    => TryGetField(vType, fieldName, out VField info)
    ? info : AddField(vType, fieldName);

    //ENSURE METHOD
    public bool TryGetMethod
    (VType vType, string methodName, int genericArity, ReadOnlySpan<VMethodParam> @params, out VMethod info)
    {
        if (vType._methods.TryGetValue(new(methodName, genericArity), out var nodeIds))
        {
            foreach (var nodeId in nodeIds)
            {
                info = InfoAt<VMethod>(nodeId);
                if (info.Equals(@params))
                    return true;
            }
        }
        info = null!;
        return false;
    }

    public bool TryGetMethod
    (VType vType, string methodName, int genericArity, ImmutableArray<VMethodNamedParam> namedParams, out VMethod info)
    {
        if (vType._methods.TryGetValue(new(methodName, genericArity), out var nodeIds))
        {
            foreach (var nodeId in nodeIds)
            {
                info = InfoAt<VMethod>(nodeId);
                if (info.Equals(namedParams))
                    return true;
            }
        }
        info = null!;
        return false;
    }
    public VMethod AddMethod
    (VType vType, string methodName, int genericArity, params ReadOnlySpan<VMethodNamedParam> namedParams)
    => AddMethod(vType, methodName, genericArity, ImmutableArray.Create(namedParams));
    public VMethod AddMethod
    (VType vType, string methodName, int genericArity, ImmutableArray<VMethodNamedParam> namedParams)
    {
        var info = new VMethod(methodName, genericArity, namedParams);
        var id = AddNode(VKind.Method, info, vType.Id);

        GenericId key = new(methodName, genericArity);
        if (vType._methods.TryGetValue(key, out var nodeIds))
            nodeIds.Add(id);
        else
            vType._methods[key] = [id];

        return info;
    }
    public VMethod EnsureMethod
    (VType vType, string methodName, int genericArity, params ReadOnlySpan<VMethodNamedParam> namedParams)
    => EnsureMethod(vType, methodName, genericArity, ImmutableArray.Create(namedParams));
    public VMethod EnsureMethod
    (VType vType, string methodName, int genericArity, ImmutableArray<VMethodNamedParam> namedParams)
    => TryGetMethod(vType, methodName, genericArity, namedParams, out var info)
    ? info : AddMethod(vType, methodName, genericArity, namedParams);
}

//VIRTUAL NODES
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
{ Root, Assembly, Nspace, Type, Field, Method, Property }

//VIRTUAL INFO
public abstract class VInfo
{
    public int Id { get; internal set; }
}
internal sealed class VRoot : VInfo { }

public interface IReadOnlyVAssembly
{
    public int Hash { get; }
    public IReadOnlyDictionary<string, int> Nspaces { get; }
}
public sealed class VAssembly : VInfo, IReadOnlyVAssembly
{
    public int Hash { get; }

    internal readonly Dictionary<string, int> _nspaces = [];
    public IReadOnlyDictionary<string, int> Nspaces => _nspaces;

    internal VAssembly(int hash)
    { Hash = hash; }
}

public interface IReadOnlyVNspace
{
    public string Name { get; }

    public IReadOnlyDictionary<string, int> NestedNspaces { get; }
    public IReadOnlyDictionary<GenericId, int> Types { get; }
}
public sealed class VNspace : VInfo, IReadOnlyVNspace
{
    public string Name { get; }

    internal readonly Dictionary<string, int> _nestedNspaces = [];
    public IReadOnlyDictionary<string, int> NestedNspaces => _nestedNspaces;

    internal readonly Dictionary<GenericId, int> _types = [];
    public IReadOnlyDictionary<GenericId, int> Types => _types;

    internal VNspace(string name)
    { Name = name; }
}

//GENERIC KEY
public readonly record struct GenericId
(string Name, int GenericArity);

//TYPE
public sealed class VTypeParam
{
}
public interface IReadOnlyVType
{
    public string Name { get; }
    public int GenericArity { get; }

    public UType Base { get; }
    public TypeKind Kind { get; }
    public ImmutableArray<VTypeParam> GenericParams { get; }

    public IReadOnlyDictionary<GenericId, int> NestedTypes { get; }
    public IReadOnlyDictionary<string, int> Fields { get; }
    public IReadOnlyDictionary<GenericId, List<int>> Methods { get; }
}
public sealed class VType : VInfo, IReadOnlyVType
{
    public string Name { get; }
    public int GenericArity { get; }

    public UType Base { get; set; } = null!;
    public TypeKind Kind { get; set; }
    public ImmutableArray<VTypeParam> GenericParams { get; set; }

    public readonly Dictionary<GenericId, int> _nestedTypes = [];
    public IReadOnlyDictionary<GenericId, int> NestedTypes => _nestedTypes;

    public readonly Dictionary<string, int> _fields = [];
    public IReadOnlyDictionary<string, int> Fields => _fields;

    public readonly Dictionary<GenericId, List<int>> _methods = [];
    public IReadOnlyDictionary<GenericId, List<int>> Methods => _methods;

    internal VType(string name, int genericArity)
    { Name = name; GenericArity = genericArity; }
}
public enum TypeKind
{ CLASS, STRUCT, INTERFACE, STATIC, ENUM }

//VFIELD
public interface IReadOnlyVField
{
    public string Name { get; }
}
public sealed class VField : VInfo, IReadOnlyVField
{
    public string Name { get; }

    internal VField(string name)
    { Name = name; }
}

//VMETHOD
public sealed class VMethodParam(UType type)
{
    public UType Type => type;

    public bool Equals(VMethodParam other)
    => Type == other.Type;
}
public readonly struct VMethodNamedParam(VMethodParam param, string name)
{
    public VMethodParam Param => param;
    public string Name => name;
}

public interface IReadOnlyVMethod
{
    public string Name { get; }
    public ImmutableArray<VMethodNamedParam> NamedParams { get; }
    public UType ReturnType { get; }
}
public sealed class VMethod : VInfo, IReadOnlyVMethod
{
    public string Name { get; }
    public int GenericArity { get; }
    public ImmutableArray<VMethodNamedParam> NamedParams { get; }

    public UType ReturnType { get; set; } = null!;

    internal VMethod(string name, int genericArity, ImmutableArray<VMethodNamedParam> namedParams)
    { Name = name; GenericArity = genericArity; NamedParams = namedParams; }
    internal bool Equals(ImmutableArray<VMethodNamedParam> namedParams)
    {
        if (NamedParams.Length != namedParams.Length) return false;
        for (int i = 0; i < namedParams.Length; i++)
        {
            if (!NamedParams[i].Param.Equals(namedParams[i].Param))
                return false;
        }

        return true;
    }
    internal bool Equals(ReadOnlySpan<VMethodParam> @params)
    {
        if (NamedParams.Length != @params.Length) return false;
        for (int i = 0; i < @params.Length; i++)
        {
            if (!NamedParams[i].Param.Equals(@params[i]))
                return false;
        }

        return true;
    }
}