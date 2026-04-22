using System.Collections.Immutable;
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
        foreach (var type in vctx.Definition.MainModule.Types)
            BindType(vctx, NspaceOf(vctx.Asm, type.Namespace).Id, type);
    }

    //>>>> BIND TYPE <<<<
    private static void BindTypeMembers(VirtualContext vctx, VComposableTypeEdit vtype)
    {
        var type = vtype.Definition;

        //FIELDS
        foreach (var field in type.Fields)
            BindField(vctx, vtype.Id, field);

        //PROPERTIES
        foreach (var prop in type.Properties)
            BindProperty(vctx, vtype.Id, prop);

        //METHODS
        foreach (var method in type.Methods)
            BindMethod(vctx, vtype.Id, method);
    }
    private static void BindType(VirtualContext vctx, int outerId, TypeDefinition type)
    {
        var name = type.Name;
        T bindGenerics<T>(T vtype) where T : VTypeEdit, IGenericEdit
        {
            foreach (var param in type.GenericParameters)
                vtype.GenericParamsMut.Add(new(param.Name));

            return vtype;
        }

        //>>>> TYPES
        VTypeEdit vtype;
        if (type.IsInterface)
            vtype = bindGenerics(
                vctx.Asm.AddInterface(outerId, GenericNameOf(name))
            );
        else if (type.IsValueType && !type.IsEnum)
            vtype = bindGenerics(
                vctx.Asm.AddStruct(outerId, GenericNameOf(name))
            );
        else
            vtype = bindGenerics(
                vctx.Asm.AddObject(outerId, GenericNameOf(name))
            );

        vtype.Definition = type;

        //VISIBILITY
        var vis = VMemberVisibility.PUBLIC;
        if (type.IsNotPublic || type.IsNestedAssembly)
            vis = VMemberVisibility.ASSEMBLY;
        else if (type.IsNestedPrivate)
            vis = VMemberVisibility.PRIVATE;
        else if (type.IsNestedFamily)
            vis = VMemberVisibility.FAMILY;
        else if (type.IsNestedFamilyOrAssembly)
            vis = VMemberVisibility.FAMILY_OR_ASSEMBLY;
        else if (type.IsNestedFamilyAndAssembly)
            vis = VMemberVisibility.FAMILY_AND_ASSEMBLY;
        vtype.Visibility = vis;

        //LAYOUT
        if (vtype is ILayoutEdit ilayout)
        {
            var lay = VTypeLayout.AUTO;
            if (type.IsSequentialLayout)
                lay = VTypeLayout.SEQUENTIAL;
            else if (type.IsExplicitLayout)
                lay = VTypeLayout.EXPLICIT;
            ilayout.Layout = lay;
        }

        //MODIFIERS
        if (vtype is IAbstractEdit iabstract && type.IsAbstract)
            iabstract.IsAbstract = true;
        if (vtype is ISealedEdit isealed && type.IsSealed)
            isealed.IsSealed = true;

        //>>>> TYPE MEMBERS
        if (vctx.Asm.IsTypeContainer(vtype.Id))
        {
            foreach (var nestedType in type.NestedTypes)
                BindType(vctx, vtype.Id, nestedType);
        }

        if (IsCompilerGenerated(type.Name))
        {
            vtype.IsCompilerGenerated = true;
            return;
        }

        if (vtype is VComposableTypeEdit vcomposable)
            BindTypeMembers(vctx, vcomposable);
    }

    //>>>> BIND FIELD <<<<
    private static void BindField(VirtualContext vctx, int typeId, FieldDefinition field)
    {
        var name = field.Name;
        if (IsCompilerGenerated(name))
            return;

        //>>>> FIELDS
        VFieldMemberEdit vfield;
        vfield = vctx.Asm.AddField(typeId, name);

        vfield.Definition = field;

        //VISIBILITY
        var vis = VMemberVisibility.PUBLIC;
        if (field.IsAssembly)
            vis = VMemberVisibility.ASSEMBLY;
        else if (field.IsPrivate)
            vis = VMemberVisibility.PRIVATE;
        else if (field.IsFamily)
            vis = VMemberVisibility.FAMILY;
        else if (field.IsFamilyOrAssembly)
            vis = VMemberVisibility.FAMILY_OR_ASSEMBLY;
        else if (field.IsFamilyAndAssembly)
            vis = VMemberVisibility.FAMILY_AND_ASSEMBLY;
        vfield.Visibility = vis;

        //MODIFIERS
        if (vfield is IStaticEdit istatic && field.IsStatic)
            istatic.IsStatic = true;
    }

    //>>>> BIND PROPERTY <<<<
    private static void BindProperty(VirtualContext vctx, int typeId, PropertyDefinition property)
    {
        var name = property.Name;
        if (IsCompilerGenerated(name))
            return;

        //>>>> PROPERTY
        VPropertyEdit vproperty;
        vproperty = vctx.Asm.AddProperty(typeId, name);

        vproperty.Definition = property;
    }

    //>>>> BIND METHOD <<<<
    private static void BindMethod(VirtualContext vctx, int typeId, MethodDefinition method)
    {
        var name = method.Name;
        if (IsCompilerGenerated(name))
            return;
        T bindGenerics<T>(T vtype) where T : VMethodMemberEdit, IGenericEdit
        {
            foreach (var param in method.GenericParameters)
                vtype.GenericParamsMut.Add(new(param.Name));

            return vtype;
        }

        //>>>> METHODS
        VMethodMemberEdit vmethod = null!;
        if (method.IsConstructor)
            vmethod = vctx.Asm.AddCtor(typeId);
        else if (method.IsGetter || method.IsSetter)
        {
            foreach (var prop in vctx.Asm.EditProperties(typeId))
            {
                if (prop.Definition.GetMethod == method)
                {
                    vmethod = vctx.Asm.AddAccessor(typeId, prop.Id, VAccessorKind.Getter);
                    prop.Getter = vmethod.Id;
                    break;
                }
                if (prop.Definition.SetMethod == method)
                {
                    vmethod = vctx.Asm.AddAccessor(typeId, prop.Id, VAccessorKind.Setter);
                    prop.Setter = vmethod.Id;
                    break;
                }
            }
            if (vmethod is null)
                throw new Exception();
        }
        else
            vmethod = bindGenerics(
                vctx.Asm.AddMethod(typeId, GenericNameOf(name))
            );

        vmethod.Definition = method;

        //VISIBILITY
        var vis = VMemberVisibility.PUBLIC;
        if (method.IsAssembly)
            vis = VMemberVisibility.ASSEMBLY;
        else if (method.IsPrivate)
            vis = VMemberVisibility.PRIVATE;
        else if (method.IsFamily)
            vis = VMemberVisibility.FAMILY;
        else if (method.IsFamilyOrAssembly)
            vis = VMemberVisibility.FAMILY_OR_ASSEMBLY;
        else if (method.IsFamilyAndAssembly)
            vis = VMemberVisibility.FAMILY_AND_ASSEMBLY;
        vmethod.Visibility = vis;

        //MODIFIERS
        if (vmethod is IStaticEdit istatic && method.IsStatic)
            istatic.IsStatic = true;
        if (vmethod is IAbstractEdit iabstract && method.IsAbstract)
            iabstract.IsAbstract = true;
        if (vmethod is IVirtualEdit ivirtual && method.IsVirtual)
            ivirtual.IsVirtual = true;
        if (vmethod is IFinalEdit ifinal && method.IsFinal)
            ifinal.IsFinal = true;
    }

    //=======================
    //     NAME RESOLVER
    //=======================
    private static VNspace NspaceOf(VAssemblyEdit vasm, ReadOnlySpan<char> nspaceFullName)
    {
        static VNspace ensureNspace(VAssemblyEdit asm, VNspace nspace, ReadOnlySpan<char> nspaceFullName, int start, int cur)
        => asm.EnsureNspace(nspace.Id, nspaceFullName[start..cur].ToString());

        VNspace vnspace = vasm.ReadGlobalNspace();
        if (nspaceFullName.Length > 0)
        {
            var start = 0;
            var cur = 0;
            foreach (var c in nspaceFullName)
            {
                if (c == '.')
                {
                    vnspace = ensureNspace(vasm, vnspace, nspaceFullName, start, cur);
                    start = cur + 1;
                }
                cur++;
            }
            if (start < nspaceFullName.Length)
                vnspace = ensureNspace(vasm, vnspace, nspaceFullName, start, cur);
        }
        return vnspace;
    }
    private static GenName GenericNameOf(ReadOnlySpan<char> name)
    {
        var index = name.IndexOf('`');
        if (index < 0)
            return new(name.ToString());

        var realName = name[..index];
        var genArity = int.Parse(name[(index + 1)..]);
        return new(realName.ToString(), genArity);
    }
    private static bool IsCompilerGenerated(string name)
    => name.Length <= 0 || name[0] == '<';
}