using System.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Parser;

//>>>> VIRTUAL VIEW <<<<
public partial interface VirtualView
{
    //>>>> DEPENDENCIES <<<<
    public IEnumerable<GlobalId> Dependencies();

    //===== ASSEMBLY =====
    public VKind KindOf(GlobalId symId);
    public VInfo ReadAt(GlobalId symId);
    public T ReadAt<T>(GlobalId symId) where T : VInfo;
    public bool TryReadAt<T>(GlobalId symId, out T read) where T : VInfo;

    //NSPACE
    public VNspace ReadGlobalNspace(int assemblyId);
    public bool TryReadNspace(GlobalId outerId, string nspaceName, out VNspace read);

    //===== MEMBERS =====
    public bool IsMember(GlobalId symId);

    //TYPE
    public bool IsType(GlobalId symId);
    public bool IsComposableType(GlobalId symId);

    public bool TryReadTypeMember(GlobalId outerId, GenName name, out VTypeMember read);
    public VTypeMember ReadTypeMember(GlobalId outerId, GenName name);

    public bool TryReadMembers(GlobalId typeId, string memberName, out IReadOnlyList<int> members);
    public IReadOnlyList<int> ReadMembers(GlobalId typeId, string memberName);
    public bool TryReadMembers(GlobalId typeId, GenName memberName, out IReadOnlyList<int> members);
    public IReadOnlyList<int> ReadMembers(GlobalId typeId, GenName memberName);

    public bool TryReadType(GlobalId outerId, GenName name, out VType read);

    public bool TryReadInterface(GlobalId outerId, GenName interfaceName, out VInterface read);

    //FIELD
    public bool IsField(GlobalId symId);

    public bool TryReadField(GlobalId typeId, string fieldName, out VField read);

    //PROPERTY
    public bool IsProperty(GlobalId symId);

    public bool TryReadProperty(GlobalId typeId, string propertyName, out VProperty read);

    //METHOD
    public bool IsMethod(GlobalId symId);

    public IEnumerable<VMethod> ReadMethodOverloads(GlobalId typeId, GenName methodName);
    public IEnumerable<VMethod> ReadMethodOverloadsByName(GlobalId typeId, string methodName);

    public IEnumerable<VCtor> ReadCtorOverloads(GlobalId typeId);

    //>>>> VIRTUAL IR <<<<
    //===== ASSEMBLY =====
    public VKind KindOf(int symId)
    => KindOf(new GlobalId(-1, symId));
    public VInfo ReadAt(int symId)
    => ReadAt(new GlobalId(-1, symId));
    public T ReadAt<T>(int symId) where T : VInfo
    => ReadAt<T>(new GlobalId(-1, symId));
    public bool TryReadAt<T>(int symId, out T read) where T : VInfo
    => TryReadAt(new GlobalId(-1, symId), out read);

    //NSPACE
    public VNspace ReadGlobalNspace()
    => ReadGlobalNspace(-1);
    public bool TryReadNspace(int outerId, string nspaceName, out VNspace read)
    => TryReadNspace(new GlobalId(-1, outerId), nspaceName, out read);

    //===== MEMBERS =====
    public bool IsMember(int symId)
    => IsMember(new GlobalId(-1, symId));

    //TYPE
    public bool IsType(int symId)
    => IsType(new GlobalId(-1, symId));
    public bool IsComposableType(int symId)
    => IsComposableType(new GlobalId(-1, symId));

    public bool TryReadTypeMember(int outerId, GenName name, out VTypeMember read)
    => TryReadTypeMember(new GlobalId(-1, outerId), name, out read);
    public VTypeMember ReadTypeMember(int outerId, GenName name)
    => ReadTypeMember(new GlobalId(-1, outerId), name);

    public bool TryReadMembers(int typeId, string memberName, out IReadOnlyList<int> members)
    => TryReadMembers(new GlobalId(-1, typeId), memberName, out members);
    public IReadOnlyList<int> ReadMembers(int typeId, string memberName)
    => ReadMembers(new GlobalId(-1, typeId), memberName);
    public bool TryReadMembers(int typeId, GenName memberName, out IReadOnlyList<int> members)
    => TryReadMembers(new GlobalId(-1, typeId), memberName, out members);
    public IReadOnlyList<int> ReadMembers(int typeId, GenName memberName)
    => ReadMembers(new GlobalId(-1, typeId), memberName);

    public bool TryReadType(int outerId, GenName name, out VType read)
    => TryReadType(new GlobalId(-1, outerId), name, out read);

    public bool TryReadInterface(int outerId, GenName interfaceName, out VInterface read)
    => TryReadInterface(new GlobalId(-1, outerId), interfaceName, out read);

    //FIELD
    public bool IsField(int symId)
    => IsField(new GlobalId(-1, symId));

    public bool TryReadField(int typeId, string fieldName, out VField read)
    => TryReadField(new GlobalId(-1, typeId), fieldName, out read);

    //PROPERTY
    public bool IsProperty(int symId)
    => IsProperty(new GlobalId(-1, symId));

    public bool TryReadProperty(int typeId, string propertyName, out VProperty read)
    => TryReadProperty(new GlobalId(-1, typeId), propertyName, out read);

    //METHOD
    public bool IsMethod(int symId)
    => IsMethod(new GlobalId(-1, symId));

    public IEnumerable<VMethod> ReadMethodOverloads(int typeId, GenName methodName)
    => ReadMethodOverloads(new GlobalId(-1, typeId), methodName);
    public IEnumerable<VMethod> ReadMethodOverloadsByName(int typeId, string methodName)
    => ReadMethodOverloadsByName(new GlobalId(-1, typeId), methodName);

    public IEnumerable<VCtor> ReadCtorOverloads(int typeId)
    => ReadCtorOverloads(new GlobalId(-1, typeId));
}
public partial class ParserProcess : VirtualView
{
    //>>>> DEPENDENCIES <<<<
    public IEnumerable<GlobalId> Dependencies()
    {
        var deps = Module.Dependencies;
        for (int i = deps.Length - 1; i >= 0; i--)
            yield return deps[i];
    }

    //===== ASSEMBLY =====
    private VAssembly AssemblyAt(int assemblyId)
    => Project.AssemblyAt(assemblyId);
    private VAssembly AssemblyAt(GlobalId symId)
    => Project.AssemblyAt(symId.AssemblyId);

    public VKind KindOf(GlobalId symId)
    => AssemblyAt(symId).KindOf(symId.LocalId);
    public VInfo ReadAt(GlobalId symId)
    => AssemblyAt(symId).ReadAt(symId.LocalId);
    public T ReadAt<T>(GlobalId symId) where T : VInfo
    => AssemblyAt(symId).ReadAt<T>(symId.LocalId);
    public bool TryReadAt<T>(GlobalId symId, out T read) where T : VInfo
    => AssemblyAt(symId).TryReadAt(symId.LocalId, out read);

    //NSPACE
    public VNspace ReadGlobalNspace(int assemblyId)
    => AssemblyAt(assemblyId).ReadGlobalNspace();
    public bool TryReadNspace(GlobalId outerId, string nspaceName, out VNspace read)
    => AssemblyAt(outerId).TryReadNspace(outerId.LocalId, nspaceName, out read);

    //===== MEMBERS =====
    public bool IsMember(GlobalId symId)
    => KindOf(symId) is not VKind.Nspace;

    //TYPE
    public bool IsType(GlobalId symId)
    => KindOf(symId) is VKind.Type or VKind.Interface;
    public bool IsComposableType(GlobalId symId)
    => KindOf(symId) is VKind.Type or VKind.Interface;

    public bool TryReadTypeMember(GlobalId outerId, GenName name, out VTypeMember read)
    => AssemblyAt(outerId).TryReadTypeMember(outerId.LocalId, name, out read);
    public VTypeMember ReadTypeMember(GlobalId outerId, GenName name)
    => AssemblyAt(outerId).ReadTypeMember(outerId.LocalId, name);

    public bool TryReadMembers(GlobalId typeId, string memberName, out IReadOnlyList<int> members)
    => AssemblyAt(typeId).TryReadMembers(typeId.LocalId, memberName, out members);
    public IReadOnlyList<int> ReadMembers(GlobalId typeId, string memberName)
    => AssemblyAt(typeId).ReadMembers(typeId.LocalId, memberName);
    public bool TryReadMembers(GlobalId typeId, GenName memberName, out IReadOnlyList<int> members)
    => AssemblyAt(typeId).TryReadMembers(typeId.LocalId, memberName, out members);
    public IReadOnlyList<int> ReadMembers(GlobalId typeId, GenName memberName)
    => AssemblyAt(typeId).ReadMembers(typeId.LocalId, memberName);

    public bool TryReadType(GlobalId outerId, GenName name, out VType read)
    => AssemblyAt(outerId).TryReadType(outerId.LocalId, name, out read);

    public bool TryReadInterface(GlobalId outerId, GenName interfaceName, out VInterface read)
    => AssemblyAt(outerId).TryReadInterface(outerId.LocalId, interfaceName, out read);

    //FIELD
    public bool IsField(GlobalId symId)
    => KindOf(symId) is VKind.Field;

    public bool TryReadField(GlobalId typeId, string fieldName, out VField read)
    => AssemblyAt(typeId).TryReadField(typeId.LocalId, fieldName, out read);

    //PROPERTY
    public bool IsProperty(GlobalId symId)
    => KindOf(symId) is VKind.Property;

    public bool TryReadProperty(GlobalId typeId, string propertyName, out VProperty read)
    => AssemblyAt(typeId).TryReadProperty(typeId.LocalId, propertyName, out read);

    //METHOD
    public bool IsMethod(GlobalId symId)
    => KindOf(symId) is VKind.Method or VKind.Ctor or VKind.Accessor;

    public IEnumerable<VMethod> ReadMethodOverloads(GlobalId typeId, GenName methodName)
    => AssemblyAt(typeId).ReadMethodOverloads(typeId.LocalId, methodName);
    public IEnumerable<VMethod> ReadMethodOverloadsByName(GlobalId typeId, string methodName)
    => AssemblyAt(typeId).ReadMethodOverloadsByName(typeId.LocalId, methodName);

    public IEnumerable<VCtor> ReadCtorOverloads(GlobalId typeId)
    => AssemblyAt(typeId).ReadCtorOverloads(typeId.LocalId);
}

//>>>> VIRTUAL CONTEXT <<<<
public interface VirtualContext : VirtualView
{
    public FileNodeId GetSourceNode(int symId);
    public bool HasSourceNode(int symId);
    public bool TryGetSourceNode(int symId, out FileNodeId nodeId);

    //===== ASSEMBLY =====
    public VInfo EditAt(int symId);
    public T EditAt<T>(int symId) where T : VInfoEdit;
    public bool TryEditAt<T>(int symId, out T edit) where T : VInfoEdit;

    //NSPACE
    public VNspaceEdit EditGlobalNspace();
    public bool TryEditNspace(int outerId, string nspaceName, out VNspaceEdit edit);
    public VNspaceEdit EnsureNspace(int outerId, string nspaceName);

    //===== MEMBERS =====
    //TYPE
    public bool TryEditTypeMember(int outerId, GenName typeName, out VTypeMemberEdit edit);
    public VTypeMemberEdit EditTypeMember(int outerId, GenName typeName);

    public bool TryEditType(int outerId, GenName typeName, out VTypeEdit edit);
    public VTypeEdit AddType(int outerId, GenName typeName);

    public bool TryEditInterface(int outerId, GenName interfaceName, out VInterfaceEdit edit);
    public VInterfaceEdit AddInterface(int outerId, GenName interfaceName);

    //FIELD
    public bool TryEditField(int typeId, string fieldName, out VFieldEdit edit);
    public VFieldEdit AddField(int typeId, string fieldName);

    //PROPERTY
    public bool TryEditProperty(int typeId, string propertyName, out VPropertyEdit edit);
    public VPropertyEdit AddProperty(int typeId, string propertyName);

    //METHOD
    public IEnumerable<VMethodEdit> EditMethodOverloads(int typeId, GenName methodName);
    public IEnumerable<VMethodEdit> EditMethodOverloadsByName(int typeId, string methodName);
    public VMethodEdit AddMethod(int typeId, GenName methodName);

    public IEnumerable<VCtorEdit> EditCtorOverloads(int typeId);
    public VCtorEdit AddCtor(int typeId);

    public VAccessorEdit AddAccessor(int typeId, int sourceId, string accessorName);
}
public partial class ParserProcess : VirtualContext
{
    private void SetSourceNode(VInfo sym)
    => VIR.SetSourceNode(sym.Id, new(File.Id, RuleInst!.NodeId));
    public FileNodeId GetSourceNode(int symId)
    => VIR.GetSourceNode(symId);
    public bool HasSourceNode(int symId)
    => VIR.HasSourceNode(symId);
    public bool TryGetSourceNode(int symId, out FileNodeId nodeId)
    => VIR.TryGetSourceNode(symId, out nodeId);

    //===== ASSEMBLY =====
    public VInfo EditAt(int symId)
    => VIR.EditAt(symId);
    public T EditAt<T>(int symId) where T : VInfoEdit
    => VIR.EditAt<T>(symId);
    public bool TryEditAt<T>(int symId, out T edit) where T : VInfoEdit
    => VIR.TryEditAt(symId, out edit);

    //NSPACE
    public VNspaceEdit EditGlobalNspace()
    => VIR.EditGlobalNspace();
    public bool TryEditNspace(int outerId, string nspaceName, out VNspaceEdit edit)
    => VIR.TryEditNspace(outerId, nspaceName, out edit);
    public VNspaceEdit EnsureNspace(int outerId, string nspaceName)
    {
        var edit = VIR.EnsureNspace(outerId, nspaceName);
        SetSourceNode(edit);

        return edit;
    }

    //===== MEMBERS =====
    //TYPE
    public bool TryEditTypeMember(int outerId, GenName typeName, out VTypeMemberEdit edit)
    => VIR.TryEditTypeMember(outerId, typeName, out edit);
    public VTypeMemberEdit EditTypeMember(int outerId, GenName typeName)
    => VIR.EditTypeMember(outerId, typeName);

    public bool TryEditType(int outerId, GenName typeName, out VTypeEdit edit)
    => VIR.TryEditType(outerId, typeName, out edit);
    public VTypeEdit AddType(int outerId, GenName typeName)
    {
        var edit = VIR.AddType(outerId, typeName);
        SetSourceNode(edit);

        return edit;
    }

    public bool TryEditInterface(int outerId, GenName interfaceName, out VInterfaceEdit edit)
    => VIR.TryEditInterface(outerId, interfaceName, out edit);
    public VInterfaceEdit AddInterface(int outerId, GenName interfaceName)
    {
        var edit = VIR.AddInterface(outerId, interfaceName);
        SetSourceNode(edit);

        return edit;
    }

    //FIELD
    public bool TryEditField(int typeId, string fieldName, out VFieldEdit edit)
    => VIR.TryEditField(typeId, fieldName, out edit);
    public VFieldEdit AddField(int typeId, string fieldName)
    {
        var edit = VIR.AddField(typeId, fieldName);
        SetSourceNode(edit);

        return edit;
    }

    //PROPERTY
    public bool TryEditProperty(int typeId, string propertyName, out VPropertyEdit edit)
    => VIR.TryEditProperty(typeId, propertyName, out edit);
    public VPropertyEdit AddProperty(int typeId, string propertyName)
    {
        var edit = VIR.AddProperty(typeId, propertyName);
        SetSourceNode(edit);

        return edit;
    }

    //METHOD
    public IEnumerable<VMethodEdit> EditMethodOverloads(int typeId, GenName methodName)
    => VIR.EditMethodOverloads(typeId, methodName);
    public IEnumerable<VMethodEdit> EditMethodOverloadsByName(int typeId, string methodName)
    => VIR.EditMethodOverloadsByName(typeId, methodName);
    public VMethodEdit AddMethod(int typeId, GenName methodName)
    {
        var edit = VIR.AddMethod(typeId, methodName);
        SetSourceNode(edit);

        return edit;
    }

    public IEnumerable<VCtorEdit> EditCtorOverloads(int typeId)
    => VIR.EditCtorOverloads(typeId);
    public VCtorEdit AddCtor(int typeId)
    {
        var edit = VIR.AddCtor(typeId);
        SetSourceNode(edit);

        return edit;
    }

    public VAccessorEdit AddAccessor(int typeId, int sourceId, string accessorName)
    {
        var edit = VIR.AddAccessor(typeId, sourceId, accessorName);
        SetSourceNode(edit);

        return edit;
    }
}