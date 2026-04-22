using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    public bool IsField(int nodeId);

    //>>> VFIELD
    public IEnumerable<VField> ReadFields(int typeId);
    public bool TryReadField(int typeId, string name, out VField read);
    public VField ReadField(int typeId, string name);
}
public partial class VAssemblyEdit
{
    //===== FIELD =====
    public bool IsField(int nodeId)
    => KindOf(nodeId) is VKind.Field;

    //>>> VFIELD
    public IEnumerable<VField> ReadFields(int typeId)
    => ReadMembers<VField>(typeId, "FIELDS");
    public bool TryReadField(int typeId, string name, out VField read)
    => TryReadMember(typeId, name, out read, "FIELDS");
    public VField ReadField(int typeId, string name)
    => ReadMember<VField>(typeId, name, "FIELDS");

    public IEnumerable<VFieldEdit> EditFields(int typeId)
    => ReadMembers<VFieldEdit>(typeId, "FIELDS");
    public bool TryEditField(int typeId, string name, out VFieldEdit edit)
    => TryReadMember(typeId, name, out edit, "FIELDS");
    public VFieldEdit EditField(int typeId, string name)
    => ReadMember<VFieldEdit>(typeId, name, "FIELDS");

    public VFieldEdit AddField(int typeId, string name)
    {
        RequireComposable(typeId, "FIELDS");
        var edit = new VFieldEdit(name);

        EditTypeMembers(typeId, name).Add(AddNode(VKind.Field, edit, typeId));
        return edit;
    }
}

//>>>> VFIELD <<<<
public interface VFieldMember : VMember, IVisibility
{
    //METADATA
    public UType Type { get; }
}
public abstract class VFieldMemberEdit : VMemberEdit, VFieldMember, IVisibilityEdit
{
    internal VFieldMemberEdit(string name) : base(name) { }

    //MODIFIERS
    public VMemberVisibility Visibility { get; set; }

    //METADATA
    public UType Type { get; internal set; } = null!;

    internal FieldDefinition Definition = null!;
}

//DEFAULT FIELD
public interface VField : VFieldMember, IStatic { }
public sealed class VFieldEdit : VFieldMemberEdit, VField, IStaticEdit
{
    internal VFieldEdit(string name) : base(name) { }

    //MODIFIERS
    public bool IsStatic { get; set; }
}