using System.Collections.Immutable;

namespace DrzSharp.Compiler.Model;

public partial class VirtualWorld
{
    //UTYPE FACTORY
    readonly Dictionary<TypeDefKey, UTypeDef> _typeDefs = [];
    public UTypeDef NewUTypeDef(int defId)
    => NewUTypeDef(defId, ImmutableArray<UType>.Empty);
    public UTypeDef NewUTypeDef(int defId, params ReadOnlySpan<UType> args)
    => NewUTypeDef(defId, ImmutableArray.Create(args));
    public UTypeDef NewUTypeDef(int defId, ImmutableArray<UType> args)
    {
        TypeDefKey key = new(defId, args);
        if (_typeDefs.TryGetValue(key, out var u))
            return u;

        return _typeDefs[key] = new(defId, args);
    }

    readonly Dictionary<TypeParamKey, UTypeParam> _typeParams = [];
    public UTypeParam NewUTypeParam(int defId, int paramId)
    {
        TypeParamKey key = new(defId, paramId);
        if (_typeParams.TryGetValue(key, out var u))
            return u;

        return _typeParams[key] = new(defId, paramId);
    }

    readonly Dictionary<ArrayTypeKey, UArrayType> _arrayTypes = [];
    public UArrayType NewUArrayType(UType type, int rank)
    {
        ArrayTypeKey key = new(type, rank);
        if (_arrayTypes.TryGetValue(key, out var u))
            return u;

        return _arrayTypes[key] = new(type, rank);
    }

    readonly Dictionary<NullableTypeKey, UNullableType> _nullableTypes = [];
    public UNullableType NewUNullableType(UType type)
    {
        NullableTypeKey key = new(type);
        if (_nullableTypes.TryGetValue(key, out var u))
            return u;

        return _nullableTypes[key] = new(type);
    }

    //UTYPE CONSTANTS
    public static UVoidType UVoid => UVoidType.Type;
    public static UNullType UNull => UNullType.Type;
}

//USAGE TYPES
public abstract class UType
{
    internal UType() { }
}

//DEFINED TYPE
readonly record struct TypeDefKey
(int DefId, ImmutableArray<UType> Args);
public class UTypeDef : UType
{
    public readonly int DefId;
    public readonly ImmutableArray<UType> Args;

    internal UTypeDef(int defId, ImmutableArray<UType> args)
    { DefId = defId; Args = args; }
}

//GENERIC TYPE
readonly record struct TypeParamKey
(int DefId, int ParamId);
public class UTypeParam : UType
{
    public readonly int DefId;
    public readonly int ParamId;

    internal UTypeParam(int defId, int paramId)
    { DefId = defId; ParamId = paramId; }
}

//ARRAY TYPE
readonly record struct ArrayTypeKey
(UType Type, int Rank);
public class UArrayType : UType
{
    public readonly UType Type;
    public readonly int Rank;

    internal UArrayType(UType type, int rank)
    { Type = type; Rank = rank; }
}

//NULLABLE TYPE
readonly record struct NullableTypeKey
(UType Type);
public class UNullableType : UType
{
    public readonly UType Type;

    internal UNullableType(UType type)
    { Type = type; }
}

//SPECIAL TYPES
public class UVoidType : UType
{
    public static readonly UVoidType Type = new();
    private UVoidType() { }
}
public class UNullType : UType
{
    public static readonly UNullType Type = new();
    private UNullType() { }
}