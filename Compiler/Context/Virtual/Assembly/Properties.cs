using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    //**VPROPERTY**
    public bool TryReadProperty(int typeId, string propertyName, out VProperty read);
}
public partial class VAssemblyEdit
{
    //**VPROPERTY**
    public bool TryReadProperty(int typeId, string propertyName, out VProperty read)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE PROPERTIES: typeId{typeId} propertyName={propertyName}");

        read = null!;
        var list = ReadMembers(typeId, propertyName);
        if (list.Count > 0 && TryReadAt(list[0], out read))
            return true;

        return false;
    }
    public bool TryEditProperty(int typeId, string propertyName, out VPropertyEdit edit)
    {
        edit = null!;
        return TryReadProperty(typeId, propertyName, out var read) && Edit(read, out edit);
    }
    public VPropertyEdit AddProperty(int typeId, string propertyName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD PROPERTIES TO A NON-COMPOSABLE TYPE: typeId{typeId} propertyName={propertyName}");

        var edit = new VPropertyEdit(propertyName);

        EditMembers(typeId, propertyName).Add(AddNode(VKind.Property, edit, typeId));
        return edit;
    }
}

//>>>> VPROPERTY <<<<
public interface VPropertyMember : VMember
{
    //METADATA
    public UType Type { get; }
    public int Getter { get; }
    public bool HasGetter();
    public int Setter { get; }
    public bool HasSetter();
}
public abstract class VPropertyMemberEdit : VMemberEdit, VPropertyMember
{
    internal VPropertyMemberEdit(string name) : base(name) { }

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
public interface VProperty : VPropertyMember { }
public sealed class VPropertyEdit : VPropertyMemberEdit, VProperty
{
    internal VPropertyEdit(string name) : base(name) { }
}