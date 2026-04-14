using System.Collections.Immutable;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler;

internal static partial class VirtualLoader
{
    //=======================
    //  BIND ASSEMBLY DATA
    //=======================
    private static void BindAssemblyData(VirtualContext vctx)
    {
        BindNspaceData(vctx, vctx.Asm.ReadGlobalNspace());
    }

    private static void LoadContainedTypes(VirtualContext vctx, VTypeContainer container)
    {
        foreach (var (_, typeId) in container.Types)
        {
            var kind = vctx.Asm.KindOf(typeId);

            if (kind == VKind.Object)
                BindObjectData(vctx, vctx.Asm.EditAt<VObjectEdit>(typeId));
            else if (kind == VKind.Struct)
                BindStructData(vctx, vctx.Asm.EditAt<VStructEdit>(typeId));
            else if (kind == VKind.Interface)
                BindInterfaceData(vctx, vctx.Asm.EditAt<VInterfaceEdit>(typeId));
        }
    }
    private static void BindNspaceData(VirtualContext vctx, VNspace vnspace)
    {
        LoadContainedTypes(vctx, vnspace);

        foreach (var (_, vnspaceId) in vnspace.Nspaces)
            BindNspaceData(vctx, vctx.Asm.ReadAt<VNspace>(vnspaceId));
    }

    //>>>> BIND TYPE DATA <<<<
    private static void LoadGenericParams(VirtualContext vctx, IGenericEdit vgeneric, Collection<GenericParameter> generics, VMethod? vmethod = null)
    {
        //GENERIC PARAMETERS
        var genBuilder = ImmutableArray.CreateBuilder<VGenParam>(generics.Count);
        foreach (var genParam in generics)
        {
            //CONSTRAINTS
            var constBuilder = ImmutableArray.CreateBuilder<UType>(genParam.Constraints.Count);
            foreach (var constraint in genParam.Constraints)
            {
                constBuilder.Add(
                    ResolveReference(vctx, constraint.ConstraintType, vmethod)
                );
            }

            //CONSTRAINT ATTRIBUTES
            genBuilder.Add(new(
                genParam.Name, constBuilder.MoveToImmutable(),
                hasParamlessCtor: genParam.HasDefaultConstructorConstraint,
                isReferenceType: genParam.HasReferenceTypeConstraint,
                isValueType: genParam.HasNotNullableValueTypeConstraint
            ));
        }
        vgeneric.GenericParams = genBuilder.MoveToImmutable();
    }
    private static TypeDefinition BindMemberTypeData(VTypeMemberEdit vtype)
    {
        var type = vtype.Definition;

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

        return type;
    }
    private static void BindComposableTypeData(VirtualContext vctx, VComposableTypeEdit vtype)
    {
        var type = vtype.Definition;

        //GENERICS
        LoadGenericParams(vctx, vtype, type.GenericParameters);

        //INTERFACES
        var interBuilder = ImmutableArray.CreateBuilder<UDeclType>(type.Interfaces.Count);
        foreach (var inter in type.Interfaces)
            interBuilder.Add(ResolveDefinition(vctx, inter.InterfaceType));

        vtype.Interfaces = interBuilder.MoveToImmutable();

        //MEMBERS
        foreach (var ctorId in vtype.Ctors)
            BindCtorData(vctx, vctx.Asm.EditAt<VCtorEdit>(ctorId));
        foreach (var (_, memberIds) in vtype.Members)
        {
            foreach (var memberId in memberIds)
            {
                var kind = vctx.Asm.KindOf(memberId);
                //FIELDS
                if (kind == VKind.Field)
                    BindFieldData(vctx, vctx.Asm.EditAt<VFieldEdit>(memberId));
                //PROPERTIES
                else if (kind == VKind.Property)
                    BindPropertyData(vctx, vctx.Asm.EditAt<VPropertyEdit>(memberId));
                //METHODS
                else if (kind == VKind.Accessor)
                    BindAccessorData(vctx, vctx.Asm.EditAt<VAccessorEdit>(memberId));
            }
        }
        foreach (var (_, genericMemberIds) in vtype.GenericMembers)
        {
            foreach (var genericMemberId in genericMemberIds)
            {
                var kind = vctx.Asm.KindOf(genericMemberId);
                if (kind == VKind.Method)
                    BindMethodData(vctx, vctx.Asm.EditAt<VMethodEdit>(genericMemberId));
            }
        }
    }
    private static void BindObjectData(VirtualContext vctx, VObjectEdit vobject)
    {
        var type = BindMemberTypeData(vobject);

        //BASE
        if (type.BaseType is not null && type.BaseType.MetadataType != MetadataType.Object)
            vobject.Base = ResolveReference(vctx, type.BaseType);

        //LAYOUT
        var lay = VTypeLayout.AUTO;
        if (type.IsSequentialLayout)
            lay = VTypeLayout.SEQUENTIAL;
        else if (type.IsExplicitLayout)
            lay = VTypeLayout.EXPLICIT;

        vobject.Layout = lay;

        //ATTRIBUTES
        vobject.IsAbstract = type.IsAbstract;
        vobject.IsSealed = type.IsSealed;

        BindComposableTypeData(vctx, vobject);
        LoadContainedTypes(vctx, vobject);
    }
    private static void BindStructData(VirtualContext vctx, VStructEdit vstruct)
    {
        var type = BindMemberTypeData(vstruct);

        //LAYOUT
        var lay = VTypeLayout.AUTO;
        if (type.IsSequentialLayout)
            lay = VTypeLayout.SEQUENTIAL;
        else if (type.IsExplicitLayout)
            lay = VTypeLayout.EXPLICIT;

        vstruct.Layout = lay;

        BindComposableTypeData(vctx, vstruct);
        LoadContainedTypes(vctx, vstruct);
    }
    private static void BindInterfaceData(VirtualContext vctx, VInterfaceEdit vinterface)
    {
        BindMemberTypeData(vinterface);
        BindComposableTypeData(vctx, vinterface);
    }

    //>>>> LOAD FIELD <<<<
    private static FieldDefinition BindMemberFieldData(VirtualContext vctx, VFieldMemberEdit vfield)
    {
        var field = vfield.Definition;

        //TYPE
        vfield.Type = ResolveReference(vctx, field.FieldType);

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

        return field;
    }
    private static void BindFieldData(VirtualContext vctx, VFieldEdit vfield)
    {
        var field = BindMemberFieldData(vctx, vfield);

        //ATTRIBUTES
        vfield.IsStatic = field.IsStatic;
    }

    //>>>> LOAD PROPERTY <<<<
    private static PropertyDefinition BindMemberPropertyData(VirtualContext vctx, VPropertyMemberEdit vproperty)
    {
        var property = vproperty.Definition;

        //TYPE
        vproperty.Type = ResolveReference(vctx, property.PropertyType);

        return property;
    }
    private static void BindPropertyData(VirtualContext vctx, VPropertyEdit vproperty)
    {
        BindMemberPropertyData(vctx, vproperty);
    }

    //>>>> LOAD METHOD <<<<
    private static MethodDefinition BindMemberMethodData(VirtualContext vctx, VMethodMemberEdit vmethod)
    {
        var method = vmethod.Definition;

        //PARAMETERS
        var parameters = method.Parameters;
        var paramBuilder = ImmutableArray.CreateBuilder<VMethodParam>(parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];

            var mod = VMethodParamMods.NONE;
            if (param.IsIn)
                mod = VMethodParamMods.IN;
            if (param.IsOut)
                mod = VMethodParamMods.OUT;

            paramBuilder.Add(new(
                param.Name, ResolveReference(vctx, param.ParameterType, vmethod), mod
            ));
        }
        vmethod.Params = paramBuilder.MoveToImmutable();

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

        return method;
    }
    private static void LoadReturnType<T>(VirtualContext vctx, T vmethod) where T : VMethodMemberEdit, IReturnableEdit
    {
        var method = vmethod.Definition;

        //RETURN TYPE
        var ret = vmethod.Definition.ReturnType;
        if (ret.MetadataType == MetadataType.Void)
            vmethod.ReturnType = UVoidType.Type;
        else
            vmethod.ReturnType = ResolveReference(vctx, ret, vmethod);
    }
    private static void BindMethodData(VirtualContext vctx, VMethodEdit vmethod)
    {
        var method = BindMemberMethodData(vctx, vmethod);

        //GENERICS
        LoadGenericParams(vctx, vmethod, method.GenericParameters, vmethod);

        //RETURN TYPE
        LoadReturnType(vctx, vmethod);

        //ATTRIBUTES
        vmethod.IsStatic = method.IsStatic;
        vmethod.IsAbstract = method.IsAbstract;
        vmethod.IsVirtual = method.IsVirtual;
    }
    private static void BindCtorData(VirtualContext vctx, VCtorEdit vctor)
    {
        var method = BindMemberMethodData(vctx, vctor);

        //ATTRIBUTES
        vctor.IsStatic = method.IsStatic;
    }
    private static void BindAccessorData(VirtualContext vctx, VAccessorEdit vaccessor)
    {
        var method = BindMemberMethodData(vctx, vaccessor);

        //RETURN TYPE
        LoadReturnType(vctx, vaccessor);

        //ATTRIBUTES
        vaccessor.IsStatic = method.IsStatic;
        vaccessor.IsAbstract = method.IsAbstract;
        vaccessor.IsVirtual = method.IsVirtual;
    }

    //=======================
    //   REFERENCE RESOLVER
    //=======================
    private static UType ResolveReference(VirtualContext vctx, TypeReference typeRef, VMethodMember? vmethod = null)
    {
        switch (typeRef)
        {
            //UN-SUPPORTED
            case OptionalModifierType modT:
                return ResolveReference(vctx, modT.ElementType, vmethod);
            case RequiredModifierType modT:
                return ResolveReference(vctx, modT.ElementType, vmethod);
            case PinnedType pinT:
                return ResolveReference(vctx, pinT.ElementType, vmethod);
            case SentinelType senT:
                return ResolveReference(vctx, senT.ElementType, vmethod);
            case FunctionPointerType:
                return UContext.GetUnsafePointerType(UVoidType.Type);
            //TYPES
            case ByReferenceType refT:
                return UContext.GetPointerType(ResolveReference(vctx, refT.ElementType, vmethod));
            case PointerType ptrT:
                return UContext.GetUnsafePointerType(ResolveReference(vctx, ptrT.ElementType, vmethod));
            case ArrayType aryT:
                return UContext.GetArrayType(ResolveReference(vctx, aryT.ElementType, vmethod), aryT.Rank);
            case GenericParameter genP:
                if (genP.Type == GenericParameterType.Method)
                {
                    if (vmethod is null)
                        throw new Exception($"METHOD GENERIC PARAMETER OUTSIDE A METHOD CONTEXT: methoName={genP.FullName}");

                    return UContext.GetGenType(new(vctx.Asm.Id, vmethod.Id), genP.Position);
                }
                else
                    return UContext.GetGenType(DefinitionToTypeId(vctx, genP.DeclaringType), genP.Position);
            default:
                return ResolveDefinition(vctx, typeRef, vmethod);
        }
    }
    private static UDeclType ResolveDefinition(VirtualContext vctx, TypeReference typeRef, VMethodMember? vmethod = null)
    {
        var decl = typeRef.DeclaringType;
        UDeclType? parent = decl is not null ? ResolveDefinition(vctx, decl, vmethod) : null;

        if (typeRef is GenericInstanceType genT)
        {
            var argsBuilder = ImmutableArray.CreateBuilder<UType>(genT.GenericArguments.Count);
            foreach (var genericArg in genT.GenericArguments)
                argsBuilder.Add(ResolveReference(vctx, genericArg, vmethod));

            return UContext.GetDeclType(DefinitionToTypeId(vctx, genT.ElementType), parent, argsBuilder.MoveToImmutable());
        }
        else
            return UContext.GetDeclType(DefinitionToTypeId(vctx, typeRef), parent);
    }

    private static VAssemblyEdit DefinitionToAssembly(VirtualContext vctx, TypeReference typeRef)
    {
        DependencyName name;
        if (typeRef.Scope is AssemblyNameReference asmRef)
            name = asmRef.DependencyName();
        else if (typeRef.Module?.Assembly != null)
            name = typeRef.Module.Assembly.DependencyName();
        else
            throw new InvalidOperationException($"Cannot resolve dependency for {typeRef.FullName}");

        return vctx.Ctx.GetDependency(name);
    }
    private static GlobalId DefinitionToTypeId(VirtualContext vctx, TypeReference typeRef)
    {
        (VAssemblyEdit vasm, int localId) ResolveOuter(TypeReference typeRef)
        {
            VAssemblyEdit vasm;
            var declRef = typeRef.DeclaringType;

            if (declRef is not null)
            {
                (vasm, int localId) = ResolveOuter(declRef);
                return (vasm, vasm.ReadTypeMember(localId, GenericNameOf(typeRef.Name)).Id);
            }

            vasm = DefinitionToAssembly(vctx, typeRef);
            var vnspace = NspaceOf(vasm, typeRef.Namespace);

            return (vasm, vasm.ReadTypeMember(vnspace.Id, GenericNameOf(typeRef.Name)).Id);
        }
        var (vasm, localId) = ResolveOuter(typeRef);
        return new(vasm.Id, localId);
    }
}