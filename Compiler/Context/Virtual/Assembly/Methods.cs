using System.Collections.Immutable;
using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    //===== METHOD =====
    public bool IsMethod(int nodeId);

    //**VMETHOD**
    public IEnumerable<VMethod> ReadMethodOverloads(int typeId, GenName methodName);
    public IEnumerable<VMethod> ReadMethodOverloadsByName(int typeId, string methodName);

    //**VCTOR**
    public IEnumerable<VCtor> ReadCtorOverloads(int typeId);

    //===== FIND METHOD =====
    public bool TryFindMethodName(int typeId, string methodName, out int methodId, params ImmutableArray<UMethodParam> parameters);
    public bool TryFindMethod(int typeId, GenName methodName, out int methodId, params ImmutableArray<UMethodParam> parameters);

    public bool TryFindCtor(int typeId, out int ctorId, params ImmutableArray<UMethodParam> parameters);
}
public partial class VAssemblyEdit
{
    //===== METHOD =====
    public bool IsMethod(int nodeId)
    => KindOf(nodeId) is VKind.Method or VKind.Ctor or VKind.Accessor;

    //**VMETHOD**
    public IEnumerable<VMethod> ReadMethodOverloads(int typeId, GenName methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE METHOD OVERLOADS: typeId{typeId}");
        
        var list = ReadMembers(typeId, methodName);
        if (list.Count < 0)
            yield break;

        foreach (var id in list)
        {
            if (TryReadAt<VMethod>(id, out var read))
                yield return read;
        }
    }
    public IEnumerable<VMethodEdit> EditMethodOverloads(int typeId, GenName methodName)
    {
        foreach (var method in ReadMethodOverloads(typeId, methodName))
        {
            yield return Edit<VMethodEdit>(method);
        }
    }
    public IEnumerable<VMethod> ReadMethodOverloadsByName(int typeId, string methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE METHOD OVERLOADS: typeId{typeId} methodName{methodName}");
        
        var type = EditAt<VTypeMemberEdit>(typeId);
        foreach (var (key, list) in (type as VComposableTypeEdit)!.GenericMembersMut)
        {
            if (key.Name == methodName)
            {
                foreach (var id in list)
                {
                    if (TryReadAt<VMethod>(id, out var read))
                        yield return read;
                }
            }
        }
    }
    public IEnumerable<VMethodEdit> EditMethodOverloadsByName(int typeId, string methodName)
    {
        foreach (var method in ReadMethodOverloadsByName(typeId, methodName))
        {
            yield return Edit<VMethodEdit>(method);
        }
    }
    public VMethodEdit AddMethod(int typeId, GenName methodName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD METHODS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var edit = new VMethodEdit(methodName);

        EditMembers(typeId, methodName).Add(AddNode(VKind.Method, edit, typeId));
        return edit;
    }

    //**VCTOR**
    public IEnumerable<VCtor> ReadCtorOverloads(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE CONSTRUCTOR OVERLOADS: typeId{typeId}");

        foreach (var ctorId in ReadAt<VComposableType>(typeId).Ctors)
            yield return ReadAt<VCtor>(ctorId);
    }
    public IEnumerable<VCtorEdit> EditCtorOverloads(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE CONSTRUCTOR OVERLOADS: typeId{typeId}");

        foreach (var ctorId in ReadAt<VComposableType>(typeId).Ctors)
            yield return EditAt<VCtorEdit>(ctorId);
    }
    public VCtorEdit AddCtor(int typeId)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD CONSTRUCTORS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var edit = new VCtorEdit();

        var type = EditAt<VComposableTypeEdit>(typeId);
        type.CtorsMut.Add(AddNode(VKind.Ctor, edit, typeId));
        return edit;
    }

    //**VACCESSOR**
    public VAccessorEdit AddAccessor(int typeId, int sourceId, string accessorName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD ACCESSORS TO A NON-COMPOSABLE TYPE: typeId{typeId}");

        var edit = new VAccessorEdit(accessorName, sourceId);

        EditMembers(typeId, accessorName).Add(AddNode(VKind.Accessor, edit, typeId));
        return edit;
    }

    //===== FIND METHOD =====
    public bool TryFindMethodName(int typeId, string methodName, out int methodId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var method in ReadMethodOverloadsByName(typeId, methodName))
        {
            if (method.ParamsEqual(parameters))
            {
                methodId = method.Id;
                return true;
            }
        }

        methodId = -1;
        return false;
    }
    public bool TryFindMethod(int typeId, GenName methodName, out int methodId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var method in ReadMethodOverloads(typeId, methodName))
        {
            if (method.ParamsEqual(parameters))
            {
                methodId = method.Id;
                return true;
            }
        }

        methodId = -1;
        return false;
    }

    public bool TryFindCtor(int typeId, out int ctorId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var ctor in ReadCtorOverloads(typeId))
        {
            if (ctor.ParamsEqual(parameters))
            {
                ctorId = ctor.Id;
                return true;
            }
        }

        ctorId = -1;
        return false;
    }
}

//>>>> VMETHOD <<<<
public interface VMethodMember : VMember, IVisibility
{
    //METADATA
    public ImmutableArray<VMethodParam> Params { get; }
    public bool ParamsEqual(params ImmutableArray<UMethodParam> parameters);
}
public abstract class VMethodMemberEdit : VMemberEdit, VMethodMember, IVisibilityEdit
{
    internal VMethodMemberEdit(string name) : base(name) { }

    //METADATA
    public ImmutableArray<VMethodParam> Params { get; set; }
    public bool ParamsEqual(params ImmutableArray<UMethodParam> parameters)
    {
        if (Params.Length != parameters.Length) return false;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!Params[i].Equals(parameters[i]))
                return false;
        }
        return true;
    }

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
public interface IReturnable { public UType ReturnType { get; } }
public interface IReturnableEdit : IReturnable { public new UType ReturnType { get; set; } }

public interface VMethod : VMethodMember, IGeneric, IReturnable, IStatic, IAbstract, IVirtual { }
public sealed class VMethodEdit : VMethodMemberEdit, VMethod, IGenericEdit, IReturnableEdit, IStaticEdit, IAbstractEdit, IVirtualEdit
{
    public int GenericArity { get; }

    internal VMethodEdit(GenName name) : base(name.Name)
    { GenericArity = name.GenericArity; }

    //METADATA
    public ImmutableArray<VGenParam> GenericParams { get; set; }
    public UType ReturnType { get; set; } = null!;

    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
}

//CONSTRUCTOR METHOD
public interface VCtor : VMethodMember, IStatic { }
public sealed class VCtorEdit : VMethodMemberEdit, VCtor, IStaticEdit
{
    internal VCtorEdit() : base(".ctor") { }

    //METADATA
    public bool IsStatic { get; set; }
}

//ACCESSOR METHOD
public interface VAccessor : VMethodMember, IReturnable, IStatic, IAbstract, IVirtual
{
    public int SourceId { get; }
}
public sealed class VAccessorEdit : VMethodMemberEdit, VAccessor, IReturnableEdit, IStaticEdit, IAbstractEdit, IVirtualEdit
{
    public int SourceId { get; }

    internal VAccessorEdit(string name, int sourceId) : base(name)
    { SourceId = sourceId; }

    //METADATA
    public UType ReturnType { get; set; } = null!;

    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
}