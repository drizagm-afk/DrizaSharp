namespace DrzSharp.Compiler.Virtual;

public abstract class UType
{ internal UType() { } }

//>>>> DECLARED TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<DeclTypeKey, UDeclType> _declTypes = [];

    public static UDeclType GetDeclType(GlobalId declId)
    => GetDeclType(null, declId);
    public static UDeclType GetDeclType(GlobalId declId, params ArrayView<UType> args)
    => GetDeclType(null, declId, args);

    public static UDeclType GetDeclType(UDeclType? parent, GlobalId declId)
    => GetDeclType(parent, declId, default);
    public static UDeclType GetDeclType(UDeclType? parent, GlobalId declId, params ArrayView<UType> args)
    {
        DeclTypeKey key = new(declId, parent, args);
        if (_declTypes.TryGetValue(key, out var u))
            return u;

        return _declTypes[key] = new(declId, parent, args);
    }
}

readonly record struct DeclTypeKey
(GlobalId DeclId, UDeclType? Parent, ArrayView<UType> Args);
public class UDeclType : UType
{
    public readonly GlobalId DeclId;
    public readonly UDeclType? Parent;
    public readonly ArrayView<UType> Args;
    internal UDeclType(GlobalId declId, UDeclType? parent, ArrayView<UType> args)
    { DeclId = declId; Parent = parent; Args = args; }
}

//>>>> GENERIC TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<GenTypeKey, UGenType> _genTypes = [];
    public static UGenType GetGenType(GlobalId defId, int paramId)
    {
        GenTypeKey key = new(defId, paramId);
        if (_genTypes.TryGetValue(key, out var u))
            return u;

        return _genTypes[key] = new(defId, paramId);
    }
}

readonly record struct GenTypeKey
(GlobalId DeclId, int ParamId);
public class UGenType : UType
{
    public readonly GlobalId DeclId;
    public readonly int ParamId;
    internal UGenType(GlobalId declId, int paramId)
    { DeclId = declId; ParamId = paramId; }
}

//>>>> ARRAY TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<ArrayTypeKey, UArrayType> _arrayTypes = [];
    public static UArrayType GetArrayType(UType type, int rank)
    {
        ArrayTypeKey key = new(type, rank);
        if (_arrayTypes.TryGetValue(key, out var u))
            return u;

        return _arrayTypes[key] = new(type, rank);
    }
}

readonly record struct ArrayTypeKey
(UType Type, int Rank);
public class UArrayType : UType
{
    public readonly UType Type;
    public readonly int Rank;
    internal UArrayType(UType type, int rank)
    { Type = type; Rank = rank; }
}

//>>>> BY REFERENCE TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<AddressTypeKey, UAddressType> _addressTypes = [];
    public static UAddressType GetAddressType(UType type)
    {
        AddressTypeKey key = new(type);
        if (_addressTypes.TryGetValue(key, out var u))
            return u;

        return _addressTypes[key] = new(type);
    }
}

readonly record struct AddressTypeKey
(UType Type);
public class UAddressType : UType
{
    public readonly UType Type;
    internal UAddressType(UType type)
    { Type = type; }
}

//>>>> POINTER TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<PointerTypeKey, UPointerType> _pointerTypes = [];
    public static UPointerType GetPointerType(UType type)
    {
        PointerTypeKey key = new(type);
        if (_pointerTypes.TryGetValue(key, out var u))
            return u;

        return _pointerTypes[key] = new(type);
    }
}

readonly record struct PointerTypeKey
(UType Type);
public class UPointerType : UType
{
    public readonly UType Type;
    internal UPointerType(UType type)
    { Type = type; }
}

//>>>> SINGLETON TYPES <<<<
public static partial class UContext
{
    public static UNullType Null => UNullType.Type;
    public static UAnonType Anon => UAnonType.Type;
}
public class UNullType : UType
{
    internal static readonly UNullType Type = new();
    private UNullType() { }
}
public class UAnonType : UType
{
    internal static readonly UAnonType Type = new();
    private UAnonType() { }
}

//>>>> DECL MEMBER <<<<
public static partial class UContext
{
    private static readonly Dictionary<DeclMemberKey, UDeclMember> _declMembers = [];
    public static UDeclMember GetDeclMember(UDeclType type, GlobalId declId)
    => GetDeclMember(type, declId, default);
    public static UDeclMember GetDeclMember(UDeclType type, GlobalId declId, params ArrayView<UType> args)
    {
        DeclMemberKey key = new(declId, type, args);
        if (_declMembers.TryGetValue(key, out var u))
            return u;

        return _declMembers[key] = new(declId, type, args);
    }
}

readonly record struct DeclMemberKey
(GlobalId DeclId, UDeclType Type, ArrayView<UType> Args);
public class UDeclMember
{
    public readonly GlobalId DeclId;
    public readonly UDeclType Type;
    public readonly ArrayView<UType> Args;
    internal UDeclMember(GlobalId declId, UDeclType type, ArrayView<UType> args)
    { DeclId = declId; Type = type; Args = args; }
}