using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial class VirtualWorld
{
    //**VMETHOD**
    public IEnumerable<IVReadOnlyMethod> ReadMethodOverloads(int typeId, GenericId methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE METHOD OVERLOADS: typeId{typeId}");

        if (!TryReadGenericMemberList(typeId, methodName, out var list))
            yield break;

        foreach (var id in list)
        {
            if (TryReadInfoAt<VMethod>(id, out var rinfo))
                yield return rinfo;
        }
    }
    public IEnumerable<VMethod> EditMethodOverloads(int typeId, GenericId methodName)
    {
        foreach (var method in ReadMethodOverloads(typeId, methodName))
        {
            yield return Edit<VMethod>(method, $"typeId={typeId} methodName={methodName}");
        }
    }

    public IEnumerable<IVReadOnlyMethod> ReadMethodNameOverloads(int typeId, string methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE METHOD OVERLOADS: typeId{typeId} methodName{methodName}");

        foreach (var (key, list) in ReadInfoAt<VType>(typeId).GenericMembersMut)
        {
            if (key.Name == methodName)
            {
                foreach (var id in list)
                {
                    if (TryReadInfoAt<VMethod>(id, out var rinfo))
                        yield return rinfo;
                }
            }
        }
    }
    public IEnumerable<VMethod> EditMethodNameOverloads(int typeId, string methodName)
    {
        foreach (var method in ReadMethodNameOverloads(typeId, methodName))
        {
            yield return Edit<VMethod>(method, $"typeId={typeId} methodName={methodName}");
        }
    }

    public VMethod AddMethod(int typeId, GenericId methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD METHODS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var info = new VMethod(methodName);

        EditGenericMemberList(typeId, methodName).Add(AddNode(VKind.Method, info, typeId));
        return info;
    }

    //**VCTOR**
    public IEnumerable<IVReadOnlyCtor> ReadCtorOverloads(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE CONSTRUCTOR OVERLOADS: typeId{typeId}");
        if (!IsKind(typeId, VKind.Type))
            throw new Exception($"A NON-CLASS TYPE DOESN'T HAVE CONSTRUCTOR OVERLOADS: typeId{typeId}");

        foreach (var ctorId in ReadInfoAt<VComposableType>(typeId).Ctors)
            yield return ReadInfoAt<VCtor>(ctorId);
    }
    public IEnumerable<VCtor> EditCtorOverloads(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE CONSTRUCTOR OVERLOADS: typeId{typeId}");

        foreach (var ctorId in ReadInfoAt<VComposableType>(typeId).Ctors)
            yield return Edit<VCtor>(ctorId, $"typeId={typeId} ctor");
    }

    public VCtor AddCtor(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD CONSTRUCTORS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var info = new VCtor();

        var type = Edit<VComposableType>(typeId, $"typeId={typeId} ctor");
        type.CtorsMut.Add(AddNode(VKind.Ctor, info, typeId));
        return info;
    }

    //**VACCESSOR**
    public VAccessor AddAccessor(int typeId, int sourceId, string accessorName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD ACCESSORS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var info = new VAccessor(accessorName, sourceId);

        EditMemberList(typeId, accessorName).Add(AddNode(VKind.Accessor, info, typeId));
        return info;
    }
}

//>>>> VMETHOD <<<<
public interface IVReadOnlyMethodBase : IVReadOnlyTypeMember, IReadVisibility
{
    //METADATA
    public ImmutableArray<VMethodParam> Params { get; }
}
public abstract class VMethodBase : VTypeMember, IVReadOnlyMethodBase, IVisibility
{
    internal VMethodBase(string name) : base(name) { }

    //METADATA
    public ImmutableArray<VMethodParam> Params { get; set; }

    public VMemberVisibility Visibility { get; set; }

    //EMIT METADATA
    internal MethodDefinition Definition { get; set; } = null!;
}

//VMETHOD PARAMETERS
public readonly struct VMethodParam
(string name, UType type, VMethodParamMods mods)
{
    public string Name => name;

    public UType Type => type;
    public VMethodParamMods Mods => mods;

    public VMethodParam(string name, UType type) : this(name, type, VMethodParamMods.NONE) { }

    //EQUALS
    public bool Equals(UMethodParam param) => Equals(param.Type, param.Mods);
    public bool Equals(UType type, VMethodParamMods mods) => Type == type && Mods == mods;
}
public enum VMethodParamMods
{ NONE, OUT, IN }

public readonly struct UMethodParam(UType type, VMethodParamMods mods)
{
    public UType Type => type;
    public VMethodParamMods Mods => mods;
}

//DEFAULT METHOD
public interface IReadReturnable { public UType ReturnType { get; } }
public interface IReturnable : IReadReturnable { public new UType ReturnType { get; set; } }

public interface IVReadOnlyMethod : IVReadOnlyMethodBase, IReadGeneric, IReadReturnable, IReadStatic, IReadAbstract, IReadVirtual { }
public sealed class VMethod : VMethodBase, IVReadOnlyMethod, IGeneric, IReturnable, IStatic, IAbstract, IVirtual
{
    public int GenericArity { get; }

    internal VMethod(GenericId name) : base(name.Name)
    { GenericArity = name.GenericArity; }

    //METADATA
    public ImmutableArray<VGenericParam> GenericParams { get; set; }
    public UType ReturnType { get; set; } = null!;

    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
}

//CONSTRUCTOR METHOD
public interface IVReadOnlyCtor : IVReadOnlyMethodBase, IReadStatic { }
public sealed class VCtor : VMethodBase, IVReadOnlyCtor, IStatic
{
    internal VCtor() : base(".ctor") { }

    //METADATA
    public bool IsStatic { get; set; }
}

//ACCESSOR METHOD
public interface IVReadOnlyAccessor : IVReadOnlyMethodBase, IReadReturnable, IReadStatic, IReadAbstract, IReadVirtual
{
    public int SourceId { get; }
}
public sealed class VAccessor : VMethodBase, IVReadOnlyAccessor, IReturnable, IStatic, IAbstract, IVirtual
{
    public int SourceId { get; }

    internal VAccessor(string name, int sourceId) : base(name)
    { SourceId = sourceId; }

    //METADATA
    public UType ReturnType { get; set; } = null!;

    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
}