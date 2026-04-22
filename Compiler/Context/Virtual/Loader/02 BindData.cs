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
    => BindContainedNspacesData(vctx, VAssembly.GlobalNspaceId);
    private static void BindContainedTypesData(VirtualContext vctx, int outerId)
    {
        foreach (var vtype in vctx.Asm.EditTypes<VTypeEdit>(outerId))
            BindTypeData(vctx, vtype);
    }
    private static void BindContainedNspacesData(VirtualContext vctx, int outerId)
    {
        BindContainedTypesData(vctx, outerId);

        foreach (var vnspace in vctx.Asm.ReadNspaces(outerId))
            BindContainedNspacesData(vctx, vnspace.Id);
    }

    //>>>> IGENERIC
    private static void BindGenericParamsData(VirtualContext vctx, IGenericEdit igeneric, Collection<GenericParameter> generics)
    {
        for (int i = 0; i < generics.Count; i++)
        {
            var genParam = generics[i];
            var vgenParam = igeneric.GenericParamsMut[i];

            //CONSTRAINTS
            foreach (var constraint in genParam.Constraints)
                vgenParam.ConstraintsMut.Add(ResolveReference(vctx, constraint.ConstraintType, igeneric as VMethod));

            vgenParam.HasParamlessCtor = genParam.HasDefaultConstructorConstraint;
            vgenParam.IsReferenceType = genParam.HasReferenceTypeConstraint;
            vgenParam.IsValueType = genParam.HasNotNullableValueTypeConstraint;
        }
    }

    //>>>> BIND TYPE DATA <<<<
    private static void BindTypeMembersData(VirtualContext vctx, int typeId)
    {
        foreach (var vmember in vctx.Asm.EditMembers<VMemberEdit>(typeId))
        {
            if (vmember is VFieldMemberEdit vfield)
                BindFieldData(vctx, vfield);
            else if (vmember is VPropertyMemberEdit vproperty)
                BindPropertyData(vctx, vproperty);
            else if (vmember is VMethodMemberEdit vmethod)
                BindMethodData(vctx, vmethod);
        }
    }
    private static void BindTypeData(VirtualContext vctx, VTypeEdit vtype)
    {
        var type = vtype.Definition;

        //GENERIC PARAMS
        if (vtype is IGenericEdit igeneric)
            BindGenericParamsData(vctx, igeneric, type.GenericParameters);

        //BASE TYPE
        if (vtype is VObjectEdit vobject)
            if (type.BaseType is not null && type.BaseType.MetadataType != MetadataType.Object)
                vobject.BaseType = ResolveReference(vctx, type.BaseType);

        if (vtype is VComposableTypeEdit vcomposable)
        {
            //INTERFACES
            foreach (var inter in type.Interfaces)
                vcomposable.InterfacesMut.Add(ResolveDefinition(vctx, inter.InterfaceType));

            //>>>> TYPE MEMBERS
            BindTypeMembersData(vctx, vcomposable.Id);
        }

        if (vctx.Asm.IsTypeContainer(vtype.Id))
            BindContainedTypesData(vctx, vtype.Id);
    }

    //>>>> BIND FIELD DATA <<<<
    private static void BindFieldData(VirtualContext vctx, VFieldMemberEdit vfield)
    {
        var field = vfield.Definition;

        //FIELD TYPE
        vfield.Type = ResolveReference(vctx, field.FieldType);
    }

    //>>>> BIND PROPERTY DATA <<<<
    private static void BindPropertyData(VirtualContext vctx, VPropertyMemberEdit vproperty)
    {
        var property = vproperty.Definition;

        //PROPERTY TYPE
        vproperty.Type = ResolveReference(vctx, property.PropertyType);
    }

    //>>>> BIND METHOD DATA <<<<
    private static void BindMethodData(VirtualContext vctx, VMethodMemberEdit vmethod)
    {
        var method = vmethod.Definition;

        //GENERIC PARAMS
        if (vmethod is IGenericEdit igeneric)
            BindGenericParamsData(vctx, igeneric, method.GenericParameters);

        //PARAMETERS
        foreach (var param in method.Parameters)
        {
            var mod = VMethodParamMods.NONE;
            if (param.IsIn)
                mod = VMethodParamMods.IN;
            else if (param.IsOut)
                mod = VMethodParamMods.OUT;

            vmethod.ParamsMut.Add(new(
                param.Name, ResolveReference(vctx, param.ParameterType, vmethod), mod
            ));
        }

        //RETURN TYPE
        if (vmethod is IReturnableEdit ireturnable)
            ireturnable.ReturnType = ResolveReference(vctx, method.ReturnType, vmethod);
    }

    //=======================
    //  REFERENCE RESOLVER
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
            case FunctionPointerType funcPtrT:
                return UContext.GetPointerType(UContext.Anon);
            //TYPES
            case ByReferenceType refT:
                return UContext.GetAddressType(ResolveReference(vctx, refT.ElementType, vmethod));
            case PointerType ptrT:
                return UContext.GetPointerType(ResolveReference(vctx, ptrT.ElementType, vmethod));
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
        TypeReference? decl = typeRef.DeclaringType;
        UDeclType? parent = decl is not null ? ResolveDefinition(vctx, decl, vmethod) : null;

        if (typeRef is GenericInstanceType genT)
        {
            var argsBuilder = ArrayBuilder.Create<UType>(genT.GenericArguments.Count);
            foreach (var genericArg in genT.GenericArguments)
                argsBuilder.Add(ResolveReference(vctx, genericArg, vmethod));

            return UContext.GetDeclType(parent, DefinitionToTypeId(vctx, genT.ElementType), argsBuilder.MoveToView());
        }
        else
            return UContext.GetDeclType(parent, DefinitionToTypeId(vctx, typeRef));
    }

    //=======================
    //  DEFINITION RESOLVER
    //=======================
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
                return (vasm, vasm.ReadType<VType>(localId, GenericNameOf(typeRef.Name)).Id);
            }

            vasm = DefinitionToAssembly(vctx, typeRef);
            var vnspace = NspaceOf(vasm, typeRef.Namespace);

            return (vasm, vasm.ReadType<VType>(vnspace.Id, GenericNameOf(typeRef.Name)).Id);
        }
        var (vasm, localId) = ResolveOuter(typeRef);
        return new(vasm.Id, localId);
    }
}