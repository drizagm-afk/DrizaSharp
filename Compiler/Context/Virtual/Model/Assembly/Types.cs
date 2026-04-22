using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public bool IsType(int nodeId);
    public bool IsComposableType(int nodeId);
    public bool IsTypeContainer(int nodeId);

    public IEnumerable<T> ReadTypes<T>(int outerId) where T : VType;
    public bool TryReadType<T>(int outerId, GenName name, out T read) where T : VType;
    public T ReadType<T>(int outerId, GenName name) where T : VType;

    //>>> VOBJECT
    public IEnumerable<VObject> ReadObjects(int outerId);
    public bool TryReadObject(int outerId, GenName name, out VObject read);
    public VObject ReadObject(int outerId, GenName name);

    //>>> VSTRUCT
    public IEnumerable<VStruct> ReadStructs(int outerId);
    public bool TryReadStruct(int outerId, GenName name, out VStruct read);
    public VStruct ReadStruct(int outerId, GenName name);

    //>>> VINTERFACE
    public IEnumerable<VInterface> ReadInterfaces(int outerId);
    public bool TryReadInterface(int outerId, GenName name, out VInterface read);
    public VInterface ReadInterface(int outerId, GenName name);

    //TYPE MEMBERS
    public IEnumerable<T> ReadMembers<T>(int typeId) where T : VMember;
    public bool TryReadMember<T>(int typeId, string name, out T read) where T : VMember;
    public T ReadMember<T>(int typeId, string name) where T : VMember;

    //FIND TYPE
    public bool TryFindType(ArrayView<string> nspaceNames, GenName name, out VType type);
    public VType FindType(ArrayView<string> nspaceNames, GenName name);
}
public partial class VAssemblyEdit
{
    //===== TYPE STRUCTURE =====
    private static VKind KindOfType<T>() where T : VType
    {
        var type = typeof(T);
        if (type == typeof(VObjectEdit))
            return VKind.Object;
        if (type == typeof(VStructEdit))
            return VKind.Struct;
        if (type == typeof(VInterfaceEdit))
            return VKind.Interface;

        throw new Exception("UNSUPPORTED TYPE");
    }
    private static T NewOfType<T>(GenName name, bool isNested) where T : VTypeEdit
    {
        VType construct()
        {
            var type = typeof(T);
            if (type == typeof(VObjectEdit))
                return new VObjectEdit(name, isNested);
            if (type == typeof(VStructEdit))
                return new VStructEdit(name, isNested);
            if (type == typeof(VInterfaceEdit))
                return new VInterfaceEdit(name, isNested);

            throw new Exception("UNSUPPORTED TYPE");
        }

        return (T)construct();
    }

    private T AddType<T>(int outerId, GenName name) where T : VTypeEdit
    {
        var kind = KindOf(outerId);

        T edit = kind switch
        {
            VKind.Nspace => NewOfType<T>(name, false),
            _ => NewOfType<T>(name, true),
        };
        var types = kind == VKind.Nspace ? EditNspaceTypes(outerId) : EditNestedTypes(outerId);

        types[name] = AddNode(KindOfType<T>(), edit, outerId);
        return edit;
    }

    //TYPE MEMBERS
    private IReadOnlyList<int> ReadTypeCtors(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._ctors ?? Empty.IdList;
    private List<int> EditTypeCtors(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._ctors ??= [];

    private IReadOnlyDictionary<string, List<int>> ReadTypeMembers(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._members ?? Empty.IdListByName;
    private Dictionary<string, List<int>> EditTypeMembers(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._members ??= [];
    private IReadOnlyList<int> ReadTypeMembers(int typeId, string name)
    {
        if (ReadTypeMembers(typeId).TryGetValue(name, out var list))
            return list;

        return Empty.IdList;
    }
    private List<int> EditTypeMembers(int typeId, string name)
    {
        var members = EditTypeMembers(typeId);
        if (members.TryGetValue(name, out var list))
            return list;

        return members[name] = [];
    }

    private IReadOnlyDictionary<GenName, List<int>> ReadTypeGenericMembers(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._genericMembers ?? Empty.IdListByGenName;
    private Dictionary<GenName, List<int>> EditTypeGenericMembers(int typeId)
    => EditAt<VComposableTypeEdit>(typeId)._genericMembers ??= [];
    private IReadOnlyList<int> ReadTypeGenericMembers(int typeId, GenName name)
    {
        if (ReadTypeGenericMembers(typeId).TryGetValue(name, out var list))
            return list;

        return Empty.IdList;
    }
    private List<int> EditTypeGenericMembers(int typeId, GenName name)
    {
        var members = EditTypeGenericMembers(typeId);
        if (members.TryGetValue(name, out var list))
            return list;

        return members[name] = [];
    }

    private IReadOnlyDictionary<GenName, int> ReadNestedTypes(int typeId)
    {
        return KindOf(typeId) switch
        {
            VKind.Object => EditAt<VObjectEdit>(typeId)._types ?? Empty.IdByGenName,
            VKind.Struct => EditAt<VStructEdit>(typeId)._types ?? Empty.IdByGenName,
            _ => throw new Exception($"NON OBJECT, STRUCT OR NSPACES SYMBOLS CANNOT CONTAIN NESTED TYPES: outerId={typeId}"),
        };
    }
    private Dictionary<GenName, int> EditNestedTypes(int typeId)
    {
        return KindOf(typeId) switch
        {
            VKind.Object => EditAt<VObjectEdit>(typeId)._types ??= [],
            VKind.Struct => EditAt<VStructEdit>(typeId)._types ??= [],
            _ => throw new Exception($"NON OBJECT, STRUCT OR NSPACES SYMBOLS CANNOT CONTAIN NESTED TYPES: outerId={typeId}"),
        };
    }

    //===== TYPE =====
    public bool IsType(int nodeId)
    => KindOf(nodeId) is VKind.Object or VKind.Struct or VKind.Interface;
    public bool IsComposableType(int nodeId)
    => KindOf(nodeId) is VKind.Object or VKind.Struct or VKind.Interface;
    public bool IsTypeContainer(int nodeId)
    => KindOf(nodeId) is VKind.Object or VKind.Struct or VKind.Nspace;

    public IEnumerable<T> ReadTypes<T>(int outerId) where T : VType
    {
        var types = KindOf(outerId) == VKind.Nspace ? ReadNspaceTypes(outerId) : ReadNestedTypes(outerId);
        foreach ((_, var id) in types)
            yield return ReadAt<T>(id);
    }
    public bool TryReadType<T>(int outerId, GenName name, out T read) where T : VType
    {
        read = default!;
        var types = KindOf(outerId) == VKind.Nspace ? ReadNspaceTypes(outerId) : ReadNestedTypes(outerId);

        return types.TryGetValue(name, out int nodeId) && TryReadAt(nodeId, out read);
    }
    public T ReadType<T>(int outerId, GenName name) where T : VType
    {
        var types = KindOf(outerId) == VKind.Nspace ? ReadNspaceTypes(outerId) : ReadNestedTypes(outerId);

        if (!types.TryGetValue(name, out int nodeId) || !TryReadAt(nodeId, out T read))
            throw new Exception($"THE TYPE DOESN'T EXIST: outerId={ReadAt(outerId).Name} typeName={name}");

        return read;
    }

    public IEnumerable<T> EditTypes<T>(int outerId) where T : VTypeEdit
    => ReadTypes<T>(outerId);
    public bool TryEditType<T>(int outerId, GenName name, out T edit) where T : VTypeEdit
    => TryReadType(outerId, name, out edit);
    public T EditType<T>(int outerId, GenName name) where T : VTypeEdit
    => ReadType<T>(outerId, name);

    //>>> VOBJECT
    public IEnumerable<VObject> ReadObjects(int outerId)
    => ReadTypes<VObject>(outerId);
    public bool TryReadObject(int outerId, GenName name, out VObject read)
    => TryReadType(outerId, name, out read);
    public VObject ReadObject(int outerId, GenName name)
    => ReadType<VObject>(outerId, name);

    public IEnumerable<VObjectEdit> EditObjects(int outerId)
    => ReadTypes<VObjectEdit>(outerId);
    public bool TryEditObject(int outerId, GenName name, out VObjectEdit read)
    => TryReadType(outerId, name, out read);
    public VObjectEdit EditObject(int outerId, GenName name)
    => ReadType<VObjectEdit>(outerId, name);

    public VObjectEdit AddObject(int outerId, GenName name)
    => AddType<VObjectEdit>(outerId, name);

    //>>> VSTRUCT
    public IEnumerable<VStruct> ReadStructs(int outerId)
    => ReadTypes<VStruct>(outerId);
    public bool TryReadStruct(int outerId, GenName name, out VStruct read)
    => TryReadType(outerId, name, out read);
    public VStruct ReadStruct(int outerId, GenName name)
    => ReadType<VStruct>(outerId, name);

    public IEnumerable<VStructEdit> EditStructs(int outerId)
    => ReadTypes<VStructEdit>(outerId);
    public bool TryEditStruct(int outerId, GenName name, out VStructEdit read)
    => TryReadType(outerId, name, out read);
    public VStructEdit EditStruct(int outerId, GenName name)
    => ReadType<VStructEdit>(outerId, name);

    public VStructEdit AddStruct(int outerId, GenName name)
    => AddType<VStructEdit>(outerId, name);

    //>>> VINTERFACE
    public IEnumerable<VInterface> ReadInterfaces(int outerId)
    => ReadTypes<VInterface>(outerId);
    public bool TryReadInterface(int outerId, GenName name, out VInterface read)
    => TryReadType(outerId, name, out read);
    public VInterface ReadInterface(int outerId, GenName name)
    => ReadType<VInterface>(outerId, name);

    public IEnumerable<VInterface> EditInterfaces(int outerId)
    => ReadTypes<VInterfaceEdit>(outerId);
    public bool TryEditInterface(int outerId, GenName name, out VInterfaceEdit read)
    => TryReadType(outerId, name, out read);
    public VInterfaceEdit EditInterface(int outerId, GenName name)
    => ReadType<VInterfaceEdit>(outerId, name);

    public VInterfaceEdit AddInterface(int outerId, GenName name)
    => AddType<VInterfaceEdit>(outerId, name);

    //TYPE MEMBERS
    private void RequireComposable(int typeId, string ctx)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"NON-COMPOSABLE TYPES DON'T HAVE {ctx}: typeName{ReadAt(typeId).Name}");
    }
    private IEnumerable<T> ReadMembers<T>(int typeId, string ctx) where T : VMember
    {
        RequireComposable(typeId, ctx);

        foreach (var id in ReadTypeCtors(typeId))
            if (TryReadAt<T>(id, out var read))
                yield return read;

        foreach ((_, var idList) in ReadTypeMembers(typeId))
            foreach (var id in idList)
                if (TryReadAt<T>(id, out var read))
                    yield return read;

        foreach ((_, var idList) in ReadTypeGenericMembers(typeId))
            foreach (var id in idList)
                if (TryReadAt<T>(id, out var read))
                    yield return read;

        if (KindOf(typeId) is VKind.Object or VKind.Struct)
        {
            foreach ((_, var id) in ReadNestedTypes(typeId))
                if (TryReadAt<T>(id, out var read))
                    yield return read;
        }
    }
    public IEnumerable<T> ReadMembers<T>(int typeId) where T : VMember
    => ReadMembers<T>(typeId, "MEMBERS");
    private bool TryReadMember<T>(int typeId, string name, out T read, string ctx) where T : VMember
    {
        RequireComposable(typeId, ctx);

        read = default!;
        var list = ReadTypeMembers(typeId, name);
        if (list.Count > 0 && TryReadAt(list[0], out read))
            return true;

        return false;
    }
    public bool TryReadMember<T>(int typeId, string name, out T read) where T : VMember
    => TryReadMember(typeId, name, out read, "MEMBERS");
    private T ReadMember<T>(int typeId, string name, string ctx) where T : VMember
    {
        RequireComposable(typeId, ctx);
        return ReadAt<T>(ReadTypeMembers(typeId, name)[0]);
    }
    public T ReadMember<T>(int typeId, string name) where T : VMember
    => ReadMember<T>(typeId, name, "MEMBERS");

    public IEnumerable<T> EditMembers<T>(int typeId) where T : VMemberEdit
    => ReadMembers<T>(typeId, "MEMBERS");
    public bool TryEditMember<T>(int typeId, string name, out T edit) where T : VMemberEdit
    => TryReadMember(typeId, name, out edit, "MEMBERS");
    public T EditMember<T>(int typeId, string name) where T : VMemberEdit
    => ReadMember<T>(typeId, name, "MEMBERS");

    //===== FIND TYPE =====
    readonly Dictionary<(ArrayView<string>, GenName), int> _typesFound = [];

    public bool TryFindType(ArrayView<string> nspaceNames, GenName name, out VType type)
    {
        var key = (nspaceNames, name);
        if (_typesFound.TryGetValue(key, out var typeId))
        {
            type = ReadAt<VType>(typeId);
            return true;
        }

        //LOOP
        type = null!;
        var nspaceId = GlobalNspaceId;
        foreach (var nspaceName in nspaceNames)
        {
            if (!TryReadNspace(nspaceId, nspaceName, out var nspace))
                return false;

            nspaceId = nspace.Id;
        }

        if (!TryReadType(nspaceId, name, out type))
            return false;

        _typesFound[key] = type.Id;
        return true;
    }
    public VType FindType(ArrayView<string> nspaceNames, GenName name)
    {
        if (!TryFindType(nspaceNames, name, out var type))
            throw new Exception("TYPE DOESN'T EXIST, USE TryFindType INSTEAD FOR FALLBACK");

        return type;
    }
}

//>>>> VTYPE <<<<
public interface VType : VMember, IVisibility
{
    public bool IsNested { get; }
}
public abstract class VTypeEdit : VMemberEdit, VType, IVisibilityEdit
{
    public bool IsNested { get; }

    internal VTypeEdit(string name, bool isNested) : base(name)
    { IsNested = isNested; }

    //MODIFIERS
    public VMemberVisibility Visibility { get; set; }

    //METADATA
    internal TypeDefinition Definition = null!;
}

//COMPOSABLE TYPE: TYPE WITH MEMBERS (Object, Struct, Interface)
public interface VComposableType : VType, IGeneric
{
    //METADATA
    public VCollection<UDeclType> Interfaces { get; }
}
public abstract class VComposableTypeEdit : VTypeEdit, VComposableType, IGenericEdit
{
    public int GenericArity { get; }
    public VCollectionEdit<VGenParamEdit> GenericParamsMut { get; }
    public VCollection<VGenParam> GenericParams => GenericParamsMut;

    internal VComposableTypeEdit(GenName name, bool isNested) : base(name.Name, isNested)
    {
        GenericArity = name.GenericArity;
        GenericParamsMut = new(GenericArity);
    }

    //STRUCTURE
    internal List<int>? _ctors;
    internal Dictionary<string, List<int>>? _members;
    internal Dictionary<GenName, List<int>>? _genericMembers;

    //METADATA
    public VCollectionEdit<UDeclType> InterfacesMut { get; } = [];
    public VCollection<UDeclType> Interfaces => InterfacesMut;
}

//OBJECT
public interface VObject : VComposableType, ILayout, IAbstract, ISealed
{
    //METADATA
    public UType? BaseType { get; }
}
public sealed class VObjectEdit : VComposableTypeEdit, VObject, ILayoutEdit, IAbstractEdit, ISealedEdit
{
    internal VObjectEdit(GenName name, bool isNested) : base(name, isNested) { }

    //STRUCTURE
    internal Dictionary<GenName, int>? _types;

    //MODIFIERS
    public VTypeLayout Layout { get; set; } = VTypeLayout.AUTO;
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }

    //METADATA
    public UType? BaseType { get; set; } = null;
}

//STRUCT
public interface VStruct : VComposableType, ILayout { }
public sealed class VStructEdit : VComposableTypeEdit, VStruct, ILayoutEdit
{
    internal VStructEdit(GenName name, bool isNested) : base(name, isNested) { }

    //STRUCTURE
    internal Dictionary<GenName, int>? _types;

    //MODIFIERS
    public VTypeLayout Layout { get; set; } = VTypeLayout.SEQUENTIAL;
}

//INTERFACE
public interface VInterface : VComposableType { }
public sealed class VInterfaceEdit : VComposableTypeEdit, VInterface
{
    internal VInterfaceEdit(GenName name, bool isNested) : base(name, isNested) { }
}