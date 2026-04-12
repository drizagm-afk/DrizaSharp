using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    //===== TYPE =====
    public bool IsComposableType(int typeId);

    public bool TryReadTypeMember(int outerId, GenName typeName, out VTypeMember read);
    public VTypeMember ReadTypeMember(int outerId, GenName typeName);

    //**VTYPE**
    public bool TryReadType(int outerId, GenName typeName, out VType read);

    //**VINTERFACE**
    public bool TryReadInterface(int outerId, GenName interfaceName, out VInterface read);

    //===== TYPE MEMBERS =====
    public bool TryReadMembers(int typeId, string memberName, out IReadOnlyList<int> members);
    public IReadOnlyList<int> ReadMembers(int typeId, string memberName);
    public bool TryReadMembers(int typeId, GenName memberName, out IReadOnlyList<int> members);
    public IReadOnlyList<int> ReadMembers(int typeId, GenName memberName);

    //===== FIND TYPE =====
    public bool TryFindType(out int typeId, params ImmutableArray<GenName> nameList);
}
public partial class VAssemblyEdit
{
    //===== TYPE =====
    public bool IsComposableType(int typeId)
    => KindOf(typeId) is VKind.Type or VKind.Interface;

    private static VKind KindOfType<T>() where T : VTypeMember
    {
        var type = typeof(T);
        if (type == typeof(VTypeEdit))
            return VKind.Type;
        if (type == typeof(VInterfaceEdit))
            return VKind.Interface;

        throw new Exception("UNSUPPORTED TYPE");
    }
    private static T NewOfType<T>(GenName typeName, bool isNested) where T : VTypeMemberEdit
    {
        VTypeMember construct()
        {
            var type = typeof(T);
            if (type == typeof(VTypeEdit))
                return new VTypeEdit(typeName, isNested);
            if (type == typeof(VInterfaceEdit))
                return new VInterfaceEdit(typeName, isNested);

            throw new Exception("UNSUPPORTED TYPE");
        }

        return (T)construct();
    }

    private bool TryReadTypeMember<T>(int outerId, GenName typeName, out T read) where T : VTypeMember
    {
        read = default!;

        //LOGIC
        var kind = KindOf(outerId);
        VTypeContainer outer;
        if (kind == VKind.Nspace)
            outer = ReadAt<VNspace>(outerId);
        else if (kind == VKind.Type)
            outer = ReadAt<VType>(outerId);
        else
            return false;

        return outer.Types.TryGetValue(typeName, out int nodeId) && TryReadAt(nodeId, out read);
    }
    private bool TryEditTypeMember<T>(int outerId, GenName typeName, out T edit) where T : VTypeMemberEdit
    {
        edit = null!;
        return TryReadTypeMember(outerId, typeName, out T read) && Edit(read, out edit);
    }
    private T ReadTypeMember<T>(int outerId, GenName typeName, string md) where T : VTypeMember
    {
        var kind = KindOf(outerId);
        VTypeContainer outer;
        if (kind == VKind.Nspace)
            outer = ReadAt<VNspace>(outerId);
        else if (kind == VKind.Type)
            outer = ReadAt<VType>(outerId);
        else
            throw new Exception($"A NON-NAMESPACE AND NON-CLASS TYPE CAN'T CONTAIN NESTED TYPES: outerId={outerId} {md}");

        if (!outer.Types.TryGetValue(typeName, out int nodeId) || !TryReadAt(nodeId, out T read))
            throw new Exception($"THE TYPE DOESN'T EXIST: outerId={outerId} {md}");

        return read;
    }
    private T EditTypeMember<T>(int outerId, GenName typeName, string md) where T : VTypeMemberEdit
    {
        var read = ReadTypeMember<T>(outerId, typeName, md);
        return Edit<T>(read);
    }
    private T AddTypeMember<T>(int outerId, GenName typeName, string md) where T : VTypeMemberEdit
    {
        var kind = KindOf(outerId);

        T edit;
        VTypeContainerEdit outer;
        if (kind == VKind.Nspace)
        {
            outer = EditAt<VNspaceEdit>(outerId);
            edit = NewOfType<T>(typeName, false);
        }
        else if (kind == VKind.Type)
        {
            outer = EditAt<VTypeEdit>(outerId);
            edit = NewOfType<T>(typeName, true);
        }
        else
            throw new Exception($"CANNOT CREATE A NEW TYPE INSIDE A NON-NAMESPACE AND NON-CLASS TYPE: outerId={outerId} {md}");

        outer.TypesMut[typeName] = AddNode(KindOfType<T>(), edit, outerId);
        return edit;
    }

    public bool TryReadTypeMember(int outerId, GenName typeName, out VTypeMember read)
    => TryReadTypeMember<VTypeMember>(outerId, typeName, out read);
    public bool TryEditTypeMember(int outerId, GenName typeName, out VTypeMemberEdit edit)
    => TryEditTypeMember<VTypeMemberEdit>(outerId, typeName, out edit);
    public VTypeMember ReadTypeMember(int outerId, GenName typeName)
    => ReadTypeMember<VTypeMember>(outerId, typeName, $"typeBaseName={typeName}");
    public VTypeMemberEdit EditTypeMember(int outerId, GenName typeName)
    => EditTypeMember<VTypeMemberEdit>(outerId, typeName, $"typeBaseName={typeName}");

    //**VTYPE**
    public bool TryReadType(int outerId, GenName typeName, out VType read)
    => TryReadTypeMember(outerId, typeName, out read);
    public bool TryEditType(int outerId, GenName typeName, out VTypeEdit edit)
    => TryEditTypeMember(outerId, typeName, out edit);
    public VTypeEdit AddType(int outerId, GenName typeName)
    => AddTypeMember<VTypeEdit>(outerId, typeName, $"typeName={typeName}");

    //**VINTERFACE**
    public bool TryReadInterface(int outerId, GenName interfaceName, out VInterface read)
    => TryReadTypeMember(outerId, interfaceName, out read);
    public bool TryEditInterface(int outerId, GenName interfaceName, out VInterfaceEdit edit)
    => TryEditTypeMember(outerId, interfaceName, out edit);
    public VInterfaceEdit AddInterface(int outerId, GenName interfaceName)
    => AddTypeMember<VInterfaceEdit>(outerId, interfaceName, $"interfaceName={interfaceName}");

    //===== TYPE MEMBERS =====
    public bool TryReadMembers(int typeId, string memberName, out IReadOnlyList<int> members)
    {
        members = ReadMembers(typeId, memberName);
        return members.Count > 0;
    }
    public IReadOnlyList<int> ReadMembers(int typeId, string memberName)
    {
        if (TryReadAt(typeId, out VComposableType type)
        && type.Members.TryGetValue(memberName, out var list))
            return list;
        
        return Empty.IdList;
    }
    private List<int> EditMembers(int typeId, string memberName)
    {
        var type = EditAt<VComposableTypeEdit>(typeId);
        if (type.MembersMut.TryGetValue(memberName, out var list))
            return list;

        return type.MembersMut[memberName] = [];
    }

    public bool TryReadMembers(int typeId, GenName memberName, out IReadOnlyList<int> members)
    {
        members = ReadMembers(typeId, memberName);
        return members.Count > 0;
    }
    public IReadOnlyList<int> ReadMembers(int typeId, GenName memberName)
    {
        if (TryReadAt(typeId, out VComposableType type)
        && type.GenericMembers.TryGetValue(memberName, out var list))
            return list;
        
        return Empty.IdList;
    }
    private List<int> EditMembers(int typeId, GenName memberName)
    {
        var type = EditAt<VComposableTypeEdit>(typeId);
        if (type.GenericMembersMut.TryGetValue(memberName, out var list))
            return list;

        return type.GenericMembersMut[memberName] = [];
    }

    //===== FIND TYPE =====
    readonly Dictionary<ImmutableArray<GenName>, int> _typesFound = [];
    public bool TryFindType(out int typeId, params ImmutableArray<GenName> nameList)
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
                else if (TryReadTypeMember<VTypeMember>(nspaceId, name, out var type))
                {
                    typeId = type.Id;
                    nspaceId = -1;
                }
                else return false;
            }
            else
            {
                if (!TryReadTypeMember<VTypeMember>(typeId, name, out var type))
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
public interface VTypeMember : VMember, IVisibility
{
    public bool IsNested { get; }
}
public abstract class VTypeMemberEdit : VMemberEdit, VTypeMember, IVisibilityEdit
{
    public bool IsNested { get; }

    internal VTypeMemberEdit(string name, bool isNested) : base(name)
    { IsNested = isNested; }

    //METADATA
    public VMemberVisibility Visibility { get; set; }

    //EMIT METADATA
    internal TypeDefinition Definition = null!;
}

//COMPOSABLE TYPE: Type with members (Class, Struct, Interface)
public interface VComposableType : VTypeMember, IGeneric
{
    //METADATA
    public ImmutableArray<UDeclType> Interfaces { get; }

    //SCHEME
    public IReadOnlyCollection<int> Ctors { get; }
    public IReadOnlyDictionary<string, List<int>> Members { get; }
    public IReadOnlyDictionary<GenName, List<int>> GenericMembers { get; }
}
public abstract class VComposableTypeEdit : VTypeMemberEdit, VComposableType, IGenericEdit
{
    public int GenericArity { get; }

    internal VComposableTypeEdit(GenName name, bool isNested) : base(name.Name, isNested)
    { GenericArity = name.GenericArity; }

    //METADATA
    public ImmutableArray<UDeclType> Interfaces { get; set; }
    public ImmutableArray<VGenParam> GenericParams { get; set; }

    //SCHEME
    List<int>? _ctors;
    public List<int> CtorsMut { get => _ctors ??= []; }
    public IReadOnlyCollection<int> Ctors => _ctors ?? Empty.IdList;

    Dictionary<string, List<int>>? _members;
    public Dictionary<string, List<int>> MembersMut { get => _members ??= []; }
    public IReadOnlyDictionary<string, List<int>> Members => _members ?? Empty.IdListByName;

    Dictionary<GenName, List<int>>? _genericMembers;
    public Dictionary<GenName, List<int>> GenericMembersMut { get => _genericMembers ??= []; }
    public IReadOnlyDictionary<GenName, List<int>> GenericMembers => _genericMembers ?? Empty.IdListByGenName;
}

//DEFAULT TYPE
public interface VType : VComposableType, VTypeContainer, IAbstract, ISealed
{
    //METADATA
    public UType? Base { get; }

    public VTypeLayout Layout { get; }
}
public sealed class VTypeEdit : VComposableTypeEdit, VType, VTypeContainerEdit, IAbstractEdit, ISealedEdit
{
    internal VTypeEdit(GenName name, bool isNested) : base(name, isNested) { }

    //METADATA
    public UType? Base { get; set; } = null;

    public VTypeLayout Layout { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }

    //SCHEME
    Dictionary<GenName, int>? _types;
    public Dictionary<GenName, int> TypesMut { get => _types ??= []; }
    public IReadOnlyDictionary<GenName, int> Types => _types ?? Empty.IdByGenName;
}
public enum VTypeLayout
{ AUTO, SEQUENTIAL, EXPLICIT }

//INTERFACE
public interface VInterface : VComposableType { }
public sealed class VInterfaceEdit : VComposableTypeEdit, VInterface
{
    internal VInterfaceEdit(GenName name, bool isNested) : base(name, isNested) { }
}