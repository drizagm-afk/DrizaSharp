using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public bool IsMethod(int nodeId);

    //>>> VMETHOD
    public IEnumerable<VMethod> ReadMethods(int typeId);
    public IEnumerable<VMethod> ReadMethodOverloads(int typeId, GenName name);
    public IEnumerable<VMethod> ReadMethodOverloadsByName(int typeId, string name);

    //>>> VCTOR
    public IEnumerable<VCtor> ReadCtors(int typeId);

    //>>> VACCESSOR
    public IEnumerable<VAccessor> ReadAccessors(int typeId);
}
public partial class VAssemblyEdit
{
    //===== METHOD =====
    public bool IsMethod(int nodeId)
    => KindOf(nodeId) is VKind.Method or VKind.Ctor or VKind.Accessor;

    //>>> VMETHOD
    public IEnumerable<VMethod> ReadMethods(int typeId)
    => ReadMembers<VMethod>(typeId, "METHODS");
    private IEnumerable<T> ReadMethodOverloads<T>(int typeId, GenName name) where T : VMethod
    {
        RequireComposable(typeId, "METHODS");

        var list = ReadTypeGenericMembers(typeId, name);
        if (list.Count < 0)
            yield break;

        foreach (var id in list)
            if (TryReadAt<T>(id, out var read))
                yield return read;
    }
    public IEnumerable<VMethod> ReadMethodOverloads(int typeId, GenName name)
    => ReadMethodOverloads<VMethod>(typeId, name);
    private IEnumerable<T> ReadMethodOverloadsByName<T>(int typeId, string name) where T : VMethod
    {
        RequireComposable(typeId, "METHODS");

        foreach (var (key, list) in ReadTypeGenericMembers(typeId))
            if (key.Name == name)
                foreach (var id in list)
                    if (TryReadAt<T>(id, out var read))
                        yield return read;
    }
    public IEnumerable<VMethod> ReadMethodOverloadsByName(int typeId, string name)
    => ReadMethodOverloadsByName<VMethod>(typeId, name);

    public IEnumerable<VMethodEdit> EditMethods(int typeId)
    => ReadMembers<VMethodEdit>(typeId, "METHODS");
    public IEnumerable<VMethodEdit> EditMethodOverloads(int typeId, GenName name)
    => ReadMethodOverloads<VMethodEdit>(typeId, name);
    public IEnumerable<VMethodEdit> EditMethodOverloadsByName(int typeId, string name)
    => ReadMethodOverloadsByName<VMethodEdit>(typeId, name);

    public VMethodEdit AddMethod(int typeId, GenName name)
    {
        RequireComposable(typeId, "METHODS");
        var edit = new VMethodEdit(name);

        EditTypeGenericMembers(typeId, name).Add(AddNode(VKind.Method, edit, typeId));
        return edit;
    }

    //>>> VCTOR
    public IEnumerable<VCtor> ReadCtors(int typeId)
    => ReadMembers<VCtor>(typeId, "CONSTRUCTORS");

    public IEnumerable<VCtorEdit> EditCtors(int typeId)
    => ReadMembers<VCtorEdit>(typeId, "CONSTRUCTORS");

    public VCtorEdit AddCtor(int typeId)
    {
        RequireComposable(typeId, "CONSTRUCTORS");
        var edit = new VCtorEdit();

        EditTypeCtors(typeId).Add(AddNode(VKind.Ctor, edit, typeId));
        return edit;
    }

    //>>> VACCESSOR
    public IEnumerable<VAccessor> ReadAccessors(int typeId)
    => ReadMembers<VAccessor>(typeId, "ACCESSORS");

    public IEnumerable<VAccessorEdit> EditAccessors(int typeId)
    => ReadMembers<VAccessorEdit>(typeId, "ACCESSORS");

    public VAccessorEdit AddAccessor(int typeId, int sourceId, VAccessorKind kind)
    {
        RequireComposable(typeId, "ACCESSORS");
        var name = $"{(kind == VAccessorKind.Getter ? "get" : "set")}_{ReadAt<VInfo>(sourceId).Name}";
        var edit = new VAccessorEdit(name, sourceId, kind);

        EditTypeMembers(typeId, name).Add(AddNode(VKind.Accessor, edit, typeId));
        return edit;
    }
}

//>>>> VMETHOD <<<<
public interface VMethodMember : VMember, IVisibility
{
    //METADATA
    public VCollection<VMethodParam> Params { get; }

    public bool ParamsEqual(params IEnumerable<UMethodParam> @params);
}
public abstract class VMethodMemberEdit : VMemberEdit, VMethodMember, IVisibilityEdit
{
    internal VMethodMemberEdit(string name) : base(name) { }

    //SIGNATURE
    public VCollectionEdit<VMethodParam> ParamsMut { get; } = [];
    public VCollection<VMethodParam> Params => ParamsMut;

    public bool ParamsEqual(params IEnumerable<UMethodParam> @params)
    {
        var i = 0;
        foreach (var param in @params)
        {
            if (Params.Count <= i || !Params[i].Equals(param))
                return false;
            i++;
        }
        return Params.Count != i;
    }

    //MODIFIERS
    public VMemberVisibility Visibility { get; set; }

    //METADATA
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

public interface VMethod : VMethodMember, IGeneric, IReturnable, IStatic, IAbstract, IVirtual, IFinal { }
public sealed class VMethodEdit : VMethodMemberEdit, VMethod, IGenericEdit, IReturnableEdit, IStaticEdit, IAbstractEdit, IVirtualEdit, IFinalEdit
{
    public int GenericArity { get; }
    public VCollectionEdit<VGenParamEdit> GenericParamsMut { get; }
    public VCollection<VGenParam> GenericParams => GenericParamsMut;

    internal VMethodEdit(GenName name) : base(name.Name)
    {
        GenericArity = name.GenericArity;
        GenericParamsMut = new(GenericArity);
    }

    //MODIFIERS
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsFinal { get; set; }

    //METADATA
    public UType ReturnType { get; set; } = null!;
}

//CONSTRUCTOR METHOD
public interface VCtor : VMethodMember, IStatic { }
public sealed class VCtorEdit : VMethodMemberEdit, VCtor, IStaticEdit
{
    internal VCtorEdit() : base(".ctor") { }

    //MODIFIERS
    public bool IsStatic { get; set; }
}

//ACCESSOR METHOD
public enum VAccessorKind { Getter, Setter }
public interface VAccessor : VMethodMember, IStatic, IAbstract, IVirtual, IFinal
{
    public int SourceId { get; }
    public VAccessorKind Kind { get; }
}
public sealed class VAccessorEdit : VMethodMemberEdit, VAccessor, IStaticEdit, IAbstractEdit, IVirtualEdit, IFinalEdit
{
    public int SourceId { get; }
    public VAccessorKind Kind { get; }

    internal VAccessorEdit(string name, int sourceId, VAccessorKind kind) : base(name)
    { SourceId = sourceId; Kind = kind; }

    //MODIFIERS
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsFinal { get; set; }
}