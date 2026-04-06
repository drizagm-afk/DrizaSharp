using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface IVReadOnlyAssembly
{
    //===== TYPE =====
    public bool IsComposableType(int typeId);

    public bool TryReadTypeBase(int outerId, GenericId typeName, out IVReadOnlyTypeBase rinfo);
    public VTypeBase ReadTypeBase(int outerId, GenericId typeName);

    //**VTYPE**
    public bool TryReadType(int outerId, GenericId typeName, out IVReadOnlyType rinfo);

    //**VINTERFACE**
    public bool TryReadInterface(int outerId, GenericId interfaceName, out IVReadOnlyInterface rinfo);

    //===== TYPE MEMBERS =====
    public bool TryReadMemberList(int typeId, string memberName, out IReadOnlyList<int> memberList);
    public bool TryReadGenericMemberList(int typeId, GenericId memberName, out IReadOnlyList<int> memberList);

    //===== FIND TYPE =====
    public bool TryFindType(out int typeId, params ImmutableArray<GenericId> nameList);
}
public partial class VAssembly
{
    //===== TYPE =====
    public bool IsComposableType(int typeId)
    => KindOf(typeId) is VKind.Type or VKind.Interface;

    private static VKind KindOfType<T>() where T : VTypeBase
    {
        var type = typeof(T);
        if (type == typeof(VType))
            return VKind.Type;
        if (type == typeof(VInterface))
            return VKind.Interface;

        throw new Exception("UNSUPPORTED TYPE");
    }
    private static T NewOfType<T>(GenericId typeName, bool isNested) where T : VTypeBase
    {
        VTypeBase construct()
        {
            var type = typeof(T);
            if (type == typeof(VType))
                return new VType(typeName, isNested);
            if (type == typeof(VInterface))
                return new VInterface(typeName, isNested);

            throw new Exception("UNSUPPORTED TYPE");
        }

        return (T)construct();
    }

    private bool TryReadTypeBase<T>(int outerId, GenericId typeName, out T rinfo) where T : IVReadOnlyTypeBase
    {
        rinfo = default!;

        //LOGIC
        var kind = KindOf(outerId);
        VTypeContainer outer;
        if (kind == VKind.Nspace)
            outer = ReadInfoAt<VNspace>(outerId);
        else if (kind == VKind.Type)
            outer = ReadInfoAt<VType>(outerId);
        else
            return false;

        return outer.Types.TryGetValue(typeName, out int nodeId) && TryReadInfoAt(nodeId, out rinfo);
    }
    private bool TryEditTypeBase<T>(int outerId, GenericId typeName, out T info) where T : VTypeBase
    {
        info = null!;
        return TryReadTypeBase(outerId, typeName, out T rinfo) && TryEdit(rinfo, out info);
    }
    private T ReadTypeBase<T>(int outerId, GenericId typeName, string md) where T : VTypeBase
    {
        var kind = KindOf(outerId);
        VTypeContainer outer;
        if (kind == VKind.Nspace)
            outer = ReadInfoAt<VNspace>(outerId);
        else if (kind == VKind.Type)
            outer = ReadInfoAt<VType>(outerId);
        else
            throw new Exception($"A NON-NAMESPACE AND NON-CLASS TYPE CAN'T CONTAIN NESTED TYPES: outerId={outerId} {md}");

        if (!outer.Types.TryGetValue(typeName, out int nodeId) || !TryReadInfoAt(nodeId, out T rinfo))
            throw new Exception($"THE TYPE DOESN'T EXIST: outerId={outerId} {md}");

        return rinfo;
    }
    private T EditTypeBase<T>(int outerId, GenericId typeName, string md) where T : VTypeBase
    {
        var rinfo = ReadTypeBase<T>(outerId, typeName, md);
        return Edit<T>(rinfo, md);
    }
    private T AddTypeBase<T>(int outerId, GenericId typeName, string md) where T : VTypeBase
    {
        var kind = KindOf(outerId);

        T info;
        VTypeContainer outer;
        if (kind == VKind.Nspace)
        {
            outer = Edit<VNspace>(outerId, $"nspaceId={outerId} {md}");
            info = NewOfType<T>(typeName, false);
        }
        else if (kind == VKind.Type)
        {
            outer = Edit<VType>(outerId, $"outerTypeId={outerId} {md}");
            info = NewOfType<T>(typeName, true);
        }
        else
            throw new Exception($"CANNOT CREATE A NEW TYPE INSIDE A NON-NAMESPACE AND NON-CLASS TYPE: outerId={outerId} {md}");

        outer.TypesMut[typeName] = AddNode(KindOfType<T>(), info, outerId);
        return info;
    }

    public bool TryReadTypeBase(int outerId, GenericId typeName, out IVReadOnlyTypeBase rinfo)
    => TryReadTypeBase<IVReadOnlyTypeBase>(outerId, typeName, out rinfo);
    public bool TryEditTypeBase(int outerId, GenericId typeName, out VTypeBase info)
    => TryEditTypeBase<VTypeBase>(outerId, typeName, out info);
    public VTypeBase ReadTypeBase(int outerId, GenericId typeName)
    => ReadTypeBase<VTypeBase>(outerId, typeName, $"typeBaseName={typeName}");
    public VTypeBase EditTypeBase(int outerId, GenericId typeName)
    => EditTypeBase<VTypeBase>(outerId, typeName, $"typeBaseName={typeName}");

    //**VTYPE**
    public bool TryReadType(int outerId, GenericId typeName, out IVReadOnlyType rinfo)
    => TryReadTypeBase(outerId, typeName, out rinfo);
    public bool TryEditType(int outerId, GenericId typeName, out VType info)
    => TryEditTypeBase(outerId, typeName, out info);
    public VType AddType(int outerId, GenericId typeName)
    => AddTypeBase<VType>(outerId, typeName, $"typeName={typeName}");

    //**VINTERFACE**
    public bool TryReadInterface(int outerId, GenericId interfaceName, out IVReadOnlyInterface rinfo)
    => TryReadTypeBase(outerId, interfaceName, out rinfo);
    public bool TryEditInterface(int outerId, GenericId interfaceName, out VInterface info)
    => TryEditTypeBase(outerId, interfaceName, out info);
    public VInterface AddInterface(int outerId, GenericId interfaceName)
    => AddTypeBase<VInterface>(outerId, interfaceName, $"interfaceName={interfaceName}");

    //===== TYPE MEMBERS =====
    public bool TryReadMemberList(int typeId, string memberName, out IReadOnlyList<int> memberList)
    {
        memberList = null!;
        if (!TryReadInfoAt(typeId, out VComposableType type))
            return false;

        if (type.Members.TryGetValue(memberName, out var list))
        {
            memberList = list;
            return true;
        }
        return false;
    }
    public bool TryEditMemberList(int typeId, string memberName, out List<int> memberList)
    {
        memberList = null!;
        if (!TryEditInfoAt<VComposableType>(typeId, out var type))
            return false;

        if (!type.MembersMut.TryGetValue(memberName, out var list))
            list = type.MembersMut[memberName] = [];

        memberList = list;
        return true;
    }
    public List<int> EditMemberList(int typeId, string memberName)
    {
        var type = Edit<VComposableType>(typeId, $"typeId={typeId} memberName={memberName}");
        if (type.MembersMut.TryGetValue(memberName, out var list))
            return list;

        return type.MembersMut[memberName] = [];
    }

    public bool TryReadGenericMemberList(int typeId, GenericId memberName, out IReadOnlyList<int> memberList)
    {
        memberList = null!;
        if (!TryReadInfoAt(typeId, out VComposableType type))
            return false;

        if (type.GenericMembers.TryGetValue(memberName, out var list))
        {
            memberList = list;
            return true;
        }
        return false;
    }
    public bool TryEditGenericMemberList(int typeId, GenericId memberName, out List<int> memberList)
    {
        memberList = null!;
        if (!TryEditInfoAt<VComposableType>(typeId, out var type))
            return false;

        if (!type.GenericMembersMut.TryGetValue(memberName, out var list))
            list = type.GenericMembersMut[memberName] = [];

        memberList = list;
        return true;
    }
    public List<int> EditGenericMemberList(int typeId, GenericId memberName)
    {
        var type = Edit<VComposableType>(typeId, $"typeId={typeId} genericMemberName={memberName}");
        if (type.GenericMembersMut.TryGetValue(memberName, out var list))
            return list;

        return type.GenericMembersMut[memberName] = [];
    }

    //===== FIND TYPE =====
    readonly Dictionary<ImmutableArray<GenericId>, int> _typesFound = [];
    public bool TryFindType(out int typeId, params ImmutableArray<GenericId> nameList)
    {
        if (_typesFound.TryGetValue(nameList, out typeId))
            return true;

        //LOOP
        typeId = -1;

        var nspaceId = GlobalNspaceId;
        foreach (var name in nameList)
        {
            if (nspaceId >= 0)
            {
                if (TryReadNspace(nspaceId, name.Name, out var nspace))
                    nspaceId = nspace.Id;
                else if (TryReadTypeBase<VTypeBase>(nspaceId, name, out var type))
                {
                    typeId = type.Id;
                    nspaceId = -1;
                }
                else return false;
            }
            else
            {
                if (!TryReadTypeBase<VTypeBase>(typeId, name, out var type))
                    return false;

                typeId = type.Id;
            }
        }

        if (typeId < 0)
            return false;

        _typesFound[nameList] = typeId;
        return true;
    }
}

//>>>> VTYPE <<<<
public interface IVReadOnlyTypeBase : IVReadOnlyMember, IReadVisibility
{
    public bool IsNested { get; }
}
public abstract class VTypeBase : VMember, IVReadOnlyTypeBase, IVisibility
{
    public bool IsNested { get; }

    internal VTypeBase(string name, bool isNested) : base(name)
    { IsNested = isNested; }

    //METADATA
    public VMemberVisibility Visibility { get; set; }

    //EMIT METADATA
    internal TypeDefinition Definition = null!;
}

//COMPOSABLE TYPE: Type with members (Class, Struct, Interface)
public interface IVReadOnlyComposableType : IVReadOnlyTypeBase, IReadGeneric
{
    //METADATA
    public ImmutableArray<UDeclType> Interfaces { get; }

    //SCHEME
    public IReadOnlyCollection<int> Ctors { get; }
    public IReadOnlyDictionary<string, List<int>> Members { get; }
    public IReadOnlyDictionary<GenericId, List<int>> GenericMembers { get; }
}
public abstract class VComposableType : VTypeBase, IVReadOnlyComposableType, IGeneric
{
    public int GenericArity { get; }

    internal VComposableType(GenericId name, bool isNested) : base(name.Name, isNested)
    { GenericArity = name.GenericArity; }

    //METADATA
    public ImmutableArray<UDeclType> Interfaces { get; set; }
    public ImmutableArray<VGenericParam> GenericParams { get; set; }

    //SCHEME
    List<int>? _ctors;
    public List<int> CtorsMut { get => _ctors ??= []; }
    public IReadOnlyCollection<int> Ctors => _ctors ?? EmptyDict.Ctors;

    Dictionary<string, List<int>>? _members;
    public Dictionary<string, List<int>> MembersMut { get => _members ??= []; }
    public IReadOnlyDictionary<string, List<int>> Members => _members ?? EmptyDict.Names;

    Dictionary<GenericId, List<int>>? _genericMembers;
    public Dictionary<GenericId, List<int>> GenericMembersMut { get => _genericMembers ??= []; }
    public IReadOnlyDictionary<GenericId, List<int>> GenericMembers => _genericMembers ?? EmptyDict.GenericNames;
}

//DEFAULT TYPE
public interface IVReadOnlyType : IVReadOnlyComposableType, IVReadOnlyTypeContainer, IReadAbstract, IReadSealed
{
    //METADATA
    public UType? Base { get; }

    public VTypeLayout Layout { get; }
}
public sealed class VType : VComposableType, IVReadOnlyType, VTypeContainer, IAbstract, ISealed
{
    internal VType(GenericId name, bool isNested) : base(name, isNested) { }

    //METADATA
    public UType? Base { get; set; } = null;

    public VTypeLayout Layout { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }

    //SCHEME
    Dictionary<GenericId, int>? _types;
    public Dictionary<GenericId, int> TypesMut { get => _types ??= []; }
    public IReadOnlyDictionary<GenericId, int> Types => _types ?? EmptyDict.Types;
}
public enum VTypeLayout
{ AUTO, SEQUENTIAL, EXPLICIT }

//INTERFACE
public interface IVReadOnlyInterface : IVReadOnlyComposableType { }
public sealed class VInterface : VComposableType, IVReadOnlyInterface
{ internal VInterface(GenericId name, bool isNested) : base(name, isNested) { } }