using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface IVReadOnlyAssembly
{
    //**VPROPERTY**
    public bool TryReadProperty(int typeId, string propertyName, out IVReadOnlyProperty rinfo);
}
public partial class VAssembly
{
    //**VPROPERTY**
    public bool TryReadProperty(int typeId, string propertyName, out IVReadOnlyProperty rinfo)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE PROPERTIES: typeId{typeId} propertyName={propertyName}");

        rinfo = null!;
        if (TryReadMemberList(typeId, propertyName, out var list)
        && list.Count > 0 && TryReadInfoAt(list[0], out rinfo))
            return true;

        return false;
    }
    public bool TryEditProperty(int typeId, string propertyName, out VProperty info)
    {
        info = null!;
        return TryReadProperty(typeId, propertyName, out var rinfo) && TryEdit(rinfo, out info);
    }

    public VProperty AddProperty(int typeId, string propertyName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD PROPERTIES TO A NON-COMPOSABLE TYPE: typeId{typeId} propertyName={propertyName}");

        var info = new VProperty(propertyName);

        EditMemberList(typeId, propertyName).Add(AddNode(VKind.Property, info, typeId));
        return info;
    }
}

//>>>> VPROPERTY <<<<
public interface IVReadOnlyPropertyBase : IVReadOnlyMember
{
    //METADATA
    public UType Type { get; }
    public int Getter { get; }
    public bool HasGetter();
    public int Setter { get; }
    public bool HasSetter();
}
public abstract class VPropertyBase : VMember, IVReadOnlyPropertyBase
{
    internal VPropertyBase(string name) : base(name) { }

    //METADATA
    public UType Type { get; set; } = null!;
    public int Getter { get; set; } = -1;
    public bool HasGetter() => Getter >= 0;
    public int Setter { get; set; } = -1;
    public bool HasSetter() => Setter >= 0;

    //EMIT METADATA
    internal PropertyDefinition Definition = null!;
}

//DEFAULT PROPERTY
public interface IVReadOnlyProperty : IVReadOnlyPropertyBase { }
public sealed class VProperty : VPropertyBase, IVReadOnlyProperty
{
    internal VProperty(string name) : base(name) { }
}