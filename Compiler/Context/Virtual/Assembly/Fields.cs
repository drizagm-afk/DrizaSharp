using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface VAssembly
{
    //===== FIELD =====
    public bool IsField(int nodeId);

    //**VFIELD**
    public bool TryReadField(int typeId, string fieldName, out VField read);
}
public partial class VAssemblyEdit
{
    //===== FIELD =====
    public bool IsField(int nodeId)
    => KindOf(nodeId) is VKind.Field;

    //**VFIELD**
    public bool TryReadField(int typeId, string fieldName, out VField read)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE FIELDS: typeId{typeId} fieldName={fieldName}");

        read = null!;
        var list = ReadMembers(typeId, fieldName);
        if (list.Count > 0 && TryReadAt(list[0], out read))
            return true;

        return false;
    }
    public bool TryEditField(int typeId, string fieldName, out VFieldEdit edit)
    {
        edit = null!;
        return TryReadField(typeId, fieldName, out var read) && Edit(read, out edit);
    }
    public VFieldEdit AddField(int typeId, string fieldName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD FIELDS TO A NON-COMPOSABLE TYPE: typeId{typeId} fieldName={fieldName}");

        var edit = new VFieldEdit(fieldName);

        EditMembers(typeId, fieldName).Add(AddNode(VKind.Field, edit, typeId));
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

    //METADATA
    public UType Type { get; internal set; } = null!;

    public VMemberVisibility Visibility { get; set; }

    //EMIT METADATA
    internal FieldDefinition Definition = null!;
}

//DEFAULT FIELD
public interface VField : VFieldMember, IStatic { }
public sealed class VFieldEdit : VFieldMemberEdit, VField, IStaticEdit
{
    internal VFieldEdit(string name) : base(name) { }

    //METADATA
    public bool IsStatic { get; set; }
}