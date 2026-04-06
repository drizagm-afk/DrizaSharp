using System.Collections.Immutable;

namespace DrzSharp.Compiler.Virtual;

public abstract class UType
{ internal UType() { } }

//>>>> DECLARED TYPE <<<<
public static partial class UContext
{
    private static readonly Dictionary<DeclTypeKey, UDeclType> _declTypes = [];
    public static UDeclType GetDeclType(GlobalId declId, UDeclType? parent)
    => GetDeclType(declId, parent, ImmutableArray<UType>.Empty);
    public static UDeclType GetDeclType(GlobalId declId, UDeclType? parent, params ImmutableArray<UType> args)
    {
        DeclTypeKey key = new(declId, parent, args);
        if (_declTypes.TryGetValue(key, out var u))
            return u;

        return _declTypes[key] = new(declId, parent, args);
    }
}

readonly record struct DeclTypeKey
(GlobalId DeclId, UDeclType? Parent, ImmutableArray<UType> Args);
public class UDeclType : UType
{
    public readonly GlobalId DeclId;
    public readonly UDeclType? Parent;
    public readonly ImmutableArray<UType> Args;
    internal UDeclType(GlobalId declId, UDeclType? parent, ImmutableArray<UType> args)
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
    private static readonly Dictionary<RefTypeKey, URefType> _refTypes = [];
    public static URefType GetRefType(UType type)
    {
        RefTypeKey key = new(type);
        if (_refTypes.TryGetValue(key, out var u))
            return u;

        return _refTypes[key] = new(type);
    }
}

readonly record struct RefTypeKey
(UType Type);
public class URefType : UType
{
    public readonly UType Type;
    internal URefType(UType type)
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
    public static UVoidType UVoid => UVoidType.Type;
    public static UNullType UNull => UNullType.Type;
    public static UAnonType UAnon => UAnonType.Type;
}

public class UVoidType : UType
{
    internal static readonly UVoidType Type = new();
    private UVoidType() { }
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