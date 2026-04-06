using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler;

internal static partial class VirtualLoader
{
    //=======================
    //     BIND ASSEMBLY
    //=======================
    private static void BindAssembly(VirtualContext vctx)
    {
        //LOAD TYPE NAMES
        foreach (var type in vctx.Definition.MainModule.Types)
            BindType(vctx, NspaceOf(vctx.Asm, type.Namespace).Id, type);
    }

    //>>>> BIND TYPE <<<<
    private static bool IsTypeGenerated(VTypeBase vtype, TypeDefinition type)
    {
        vtype.Definition = type;
        return vtype.IsCompilerGenerated = IsCompilerGenerated(type.Name);
    }
    private static void BindMembers(VirtualContext vctx, VComposableType vtype)
    {
        var type = vtype.Definition;

        //**FIELDS**
        foreach (var field in type.Fields)
            BindField(vctx, vtype.Id, field);

        //**PROPERTIES**
        foreach (var prop in type.Properties)
            BindProperty(vctx, vtype.Id, prop);

        //**METHODS**
        foreach (var method in type.Methods)
            BindMethod(vctx, vtype.Id, method);
    }
    private static void BindType(VirtualContext vctx, int outerId, TypeDefinition type)
    {
        var name = type.Name;
        //===== SPECIAL TYPES =====
        if (type.IsInterface)
        {
            BindInterface(vctx, outerId, type);
            return;
        }

        //===== BASE TYPE =====
        var vtype = vctx.Asm.AddType(outerId, GenericNameOf(name));

        //**NESTED TYPES**
        foreach (var nestedType in type.NestedTypes)
            BindType(vctx, vtype.Id, nestedType);

        if (IsTypeGenerated(vtype, type))
            return;

        BindMembers(vctx, vtype);
    }
    private static void BindInterface(VirtualContext vctx, int outerId, TypeDefinition type)
    {
        var vtype = vctx.Asm.AddInterface(outerId, GenericNameOf(type.Name));

        if (IsTypeGenerated(vtype, type))
            return;

        BindMembers(vctx, vtype);
    }

    //>>>> BIND FIELD <<<<
    private static void BindFieldBase(VFieldBase vfield, FieldDefinition field)
    {
        vfield.Definition = field;
    }
    private static void BindField(VirtualContext vctx, int typeId, FieldDefinition field)
    {
        var name = field.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL FIELDS =====

        //===== BASE FIELD =====
        var vfield = vctx.Asm.AddField(typeId, name);
        BindFieldBase(vfield, field);
    }

    //>>>> BIND PROPERTY <<<<
    private static void BindPropertyBase(VirtualContext vctx, int typeId, VPropertyBase vproperty, PropertyDefinition property)
    {
        vproperty.Definition = property;

        //BIND ACCESSORS
        int bind(MethodDefinition method)
        => BindAccessor(vctx, typeId, vproperty.Id, method);

        if (property.GetMethod is not null)
            vproperty.Getter = bind(property.GetMethod);
        if (property.SetMethod is not null)
            vproperty.Setter = bind(property.SetMethod);
    }
    private static void BindProperty(VirtualContext vctx, int typeId, PropertyDefinition property)
    {
        var name = property.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL PROPERTIES =====

        //===== BASE PROPERTY =====
        var vproperty = vctx.Asm.AddProperty(typeId, name);
        BindPropertyBase(vctx, typeId, vproperty, property);
    }

    //>>>> BIND METHOD <<<<
    private static void BindMethodBase(VMethodBase vmethod, MethodDefinition method)
    {
        vmethod.Definition = method;
    }
    private static void BindMethod(VirtualContext vctx, int typeId, MethodDefinition method)
    {
        var name = method.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL METHODS =====
        if (method.IsConstructor)
        {
            BindCtor(vctx, typeId, method);
            return;
        }
        else if (method.IsGetter || method.IsSetter)
            return;

        //===== BASE METHOD =====
        var vmethod = vctx.Asm.AddMethod(typeId, GenericNameOf(name));
        BindMethodBase(vmethod, method);
    }
    private static void BindCtor(VirtualContext vctx, int typeId, MethodDefinition method)
    {
        var vCtor = vctx.Asm.AddCtor(typeId);
        BindMethodBase(vCtor, method);
    }
    private static int BindAccessor(VirtualContext vctx, int typeId, int sourceId, MethodDefinition method)
    {
        var vAccessor = vctx.Asm.AddAccessor(typeId, sourceId, method.Name);
        BindMethodBase(vAccessor, method);

        return vAccessor.Id;
    }

    //=======================
    //     NAME RESOLVER
    //=======================
    private static VNspace NspaceOf(VAssembly vasm, string nspaceFullName)
    {
        VNspace vnspace = vasm.EditGlobalNspace();
        if (nspaceFullName != string.Empty)
        {
            foreach (var nspaceName in nspaceFullName.Split('.'))
                vnspace = vasm.EnsureNspace(vnspace.Id, nspaceName);
        }
        return vnspace;
    }
    private static GenericId GenericNameOf(string name)
    {
        var index = name.IndexOf('`');
        if (index < 0)
            return new GenericId(name);

        var str = name[..index];
        var arity = int.Parse(name[(index + 1)..]);

        return new GenericId(str, arity);
    }
    private static bool IsCompilerGenerated(string name)
    => name.Length <= 0 || name[0] == '<';
}