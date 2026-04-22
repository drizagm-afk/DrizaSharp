using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public bool IsProperty(int nodeId);

    //>>> VPROPERTY
    public IEnumerable<VProperty> ReadProperties(int typeId);
    public bool TryReadProperty(int typeId, string name, out VProperty read);
    public VProperty ReadProperty(int typeId, string name);
}
public partial class VAssemblyEdit
{
    //===== PROPERTY =====
    public bool IsProperty(int nodeId)
    => KindOf(nodeId) is VKind.Property;

    //>>> VPROPERTY
    public IEnumerable<VProperty> ReadProperties(int typeId)
    => ReadMembers<VProperty>(typeId, "PROPERTIES");
    public bool TryReadProperty(int typeId, string name, out VProperty read)
    => TryReadMember(typeId, name, out read, "PROPERTIES");
    public VProperty ReadProperty(int typeId, string name)
    => ReadMember<VProperty>(typeId, name, "PROPERTIES");

    public IEnumerable<VPropertyEdit> EditProperties(int typeId)
    => ReadMembers<VPropertyEdit>(typeId, "PROPERTIES");
    public bool TryEditProperty(int typeId, string name, out VPropertyEdit edit)
    => TryReadMember(typeId, name, out edit, "PROPERTIES");
    public VPropertyEdit EditProperty(int typeId, string name)
    => ReadMember<VPropertyEdit>(typeId, name, "PROPERTIES");

    public VPropertyEdit AddProperty(int typeId, string name)
    {
        RequireComposable(typeId, "PROPERTIES");
        var edit = new VPropertyEdit(name);

        EditTypeMembers(typeId, name).Add(AddNode(VKind.Property, edit, typeId));
        return edit;
    }
}

//>>>> VPROPERTY <<<<
public interface VPropertyMember : VMember
{
    public int Getter { get; }
    public bool HasGetter();
    public int Setter { get; }
    public bool HasSetter();

    //METADATA
    public UType Type { get; }
}
public abstract class VPropertyMemberEdit : VMemberEdit, VPropertyMember
{
    internal VPropertyMemberEdit(string name) : base(name) { }

    public int Getter { get; set; } = -1;
    public bool HasGetter() => Getter >= 0;
    public int Setter { get; set; } = -1;
    public bool HasSetter() => Setter >= 0;

    //METADATA
    public UType Type { get; set; } = null!;

    internal PropertyDefinition Definition = null!;
}

//DEFAULT PROPERTY
public interface VProperty : VPropertyMember { }
public sealed class VPropertyEdit : VPropertyMemberEdit, VProperty
{
    internal VPropertyEdit(string name) : base(name) { }
}