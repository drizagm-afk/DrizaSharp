using Mono.Cecil;

namespace DrzSharp.Compiler.Virtual;

public partial interface IVReadOnlyAssembly
{
    //**VFIELD**
    public bool TryReadField(int typeId, string fieldName, out IVReadOnlyField rinfo);
}
public partial class VAssembly
{
    //**VFIELD**
    public bool TryReadField(int typeId, string fieldName, out IVReadOnlyField rinfo)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"A NON-COMPOSABLE TYPE DOESN'T HAVE FIELDS: typeId{typeId} fieldName={fieldName}");

        rinfo = null!;
        if (TryReadMemberList(typeId, fieldName, out var list)
        && list.Count > 0 && TryReadInfoAt(list[0], out rinfo))
            return true;

        return false;
    }
    public bool TryEditField(int typeId, string fieldName, out VField info)
    {
        info = null!;
        return TryReadField(typeId, fieldName, out var rinfo) && TryEdit(rinfo, out info);
    }

    public VField AddField(int typeId, string fieldName)
    {
        if (!IsComposableType(typeId))
            throw new Exception($"CANNOT ADD FIELDS TO A NON-COMPOSABLE TYPE: typeId{typeId} fieldName={fieldName}");

        var info = new VField(fieldName);

        EditMemberList(typeId, fieldName).Add(AddNode(VKind.Field, info, typeId));
        return info;
    }
}

//>>>> VFIELD <<<<
public interface IVReadOnlyFieldBase : IVReadOnlyMember, IReadVisibility
{
    //METADATA
    public UType Type { get; }
}
public abstract class VFieldBase : VMember, IVReadOnlyFieldBase, IVisibility
{
    internal VFieldBase(string name) : base(name) { }

    //METADATA
    public UType Type { get; internal set; } = null!;

    public VMemberVisibility Visibility { get; set; }

    //EMIT METADATA
    internal FieldDefinition Definition = null!;
}

//DEFAULT FIELD
public interface IVReadOnlyField : IVReadOnlyFieldBase, IReadStatic { }
public sealed class VField : VFieldBase, IVReadOnlyField, IStatic
{
    internal VField(string name) : base(name) { }

    //METADATA
    public bool IsStatic { get; set; }
}