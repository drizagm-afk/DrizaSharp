using DrzSharp.Compiler.Virtual;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    //=======================
    //  DEFINE VIRTUAL DATA
    //=======================
    private partial void DefineVirtualData()
    => DefineContainedNspacesData(VAssembly.GlobalNspaceId);
    private void DefineContainedTypesData(int outerId)
    {
        foreach (var type in VIR.EditTypes<VTypeEdit>(outerId))
            DefineTypeData(type);
    }
    private void DefineContainedNspacesData(int outerId)
    {
        DefineContainedTypesData(outerId);

        foreach (var nspace in VIR.ReadNspaces(outerId))
            DefineContainedNspacesData(nspace.Id);
    }

    //>>>> IGENERIC
    private void DefineGenericParamsData(IGeneric igeneric, Collection<GenericParameter> generics)
    {
        for (int i = 0; i < generics.Count; i++)
        {
            var genParam = igeneric.GenericParams[i];
            var genParamDef = generics[i];

            //CONSTRAINTS
            foreach (var constraint in genParam.Constraints)
                genParamDef.Constraints.Add(new(ResolveUsageType(constraint)));

            genParamDef.HasDefaultConstructorConstraint = genParam.HasParamlessCtor;
            genParamDef.HasReferenceTypeConstraint = genParam.IsReferenceType;
            genParamDef.HasNotNullableValueTypeConstraint = genParam.IsValueType;
        }
    }

    //>>>> DEFINE TYPE DATA <<<<
    private void DefineTypeMembersData(int typeId)
    {
        foreach (var member in VIR.EditMembers<VMemberEdit>(typeId))
        {
            if (member is VFieldMemberEdit field)
                DefineFieldData(field);
            if (member is VPropertyMemberEdit property)
                DefinePropertyData(property);
            if (member is VMethodMemberEdit method)
                DefineMethodData(method);
        }
    }
    private void DefineTypeData(VTypeEdit type)
    {
        var typeDef = type.Definition;

        //GENERIC PARAMS
        if (type is IGeneric igeneric)
            DefineGenericParamsData(igeneric, typeDef.GenericParameters);

        //BASE TYPE
        if (type is VObject objectType)
        {
            if (objectType.BaseType is UType baseType)
                typeDef.BaseType = ResolveUsageType(baseType);
            else
                typeDef.BaseType = REF_OBJECT;
        }

        if (type is VComposableType composable)
        {
            foreach (var inter in composable.Interfaces)
                typeDef.Interfaces.Add(new(ResolveUsageType(inter)));

            //>>>> TYPE MEMBERS
            DefineTypeMembersData(composable.Id);
        }
    }

    //>>>> DEFINE FIELD DATA <<<<
    private void DefineFieldData(VFieldMemberEdit field)
    {
        var fieldDef = field.Definition;

        //FIELD TYPE
        fieldDef.FieldType = ResolveUsageType(field.Type);
    }

    //>>>> DEFINE PROPERTY DATA <<<<
    private void DefinePropertyData(VPropertyMemberEdit property)
    {
        var propertyDef = property.Definition;

        //PROPERTY TYPE
        propertyDef.PropertyType = ResolveUsageType(property.Type);
    }

    //>>>> DEFINE METHOD DATA <<<<
    private void DefineMethodData(VMethodMemberEdit method)
    {
        var methodDef = method.Definition;

        //GENERIC PARAMS
        if (method is IGeneric igeneric)
            DefineGenericParamsData(igeneric, methodDef.GenericParameters);

        //PARAMETERS
        foreach (var param in method.Params)
        {
            ParameterAttributes attr = ParameterAttributes.None;
            if (param.Mods == VMethodParamMods.OUT)
                attr |= ParameterAttributes.Out;
            else if (param.Mods == VMethodParamMods.IN)
                attr |= ParameterAttributes.In;

            methodDef.Parameters.Add(new(param.Name, attr, ResolveUsageType(param.Type)));
        }

        //RETURN TYPE
        if (method is IReturnable ireturnable)
            methodDef.ReturnType = ResolveUsageType(ireturnable.ReturnType);
    }

    //=======================
    //    UTYPE RESOLVER
    //=======================
    private readonly Dictionary<UType, TypeReference> _refByUsage = [];
    private TypeReference ResolveUsageType(UType type)
    {
        //CACHED REFERENCE
        if (_refByUsage.TryGetValue(type, out var typeRef))
            return typeRef;

        //USAGE REFERENCE
        switch (type)
        {
            case UDeclType declType:
                typeRef = ResolveDeclaredType(declType);
                break;
            case UGenType genType:
                var decl = ReadAt(genType.DeclId);
                if (decl is VMethodEdit method)
                    typeRef = method.Definition.GenericParameters[genType.ParamId];
                else if (decl is VComposableTypeEdit compType)
                    typeRef = compType.Definition.GenericParameters[genType.ParamId];
                else
                    throw new Exception();

                break;
            case UArrayType aryType:
                typeRef = new ArrayType(ResolveUsageType(aryType.Type), aryType.Rank);
                break;
            case UAddressType addressType:
                typeRef = new ByReferenceType(ResolveUsageType(addressType.Type));
                break;
            case UPointerType ptrType:
                typeRef = new PointerType(ResolveUsageType(ptrType.Type));
                break;

            default:
                throw new Exception();
        }
        if (type is not UGenType)
            typeRef = _module.ImportReference(typeRef);

        return _refByUsage[type] = typeRef;
    }
    private TypeReference ResolveDeclaredType(UDeclType type)
    {
        //DECLARATION REFERENCE
        var typeRef = ReferenceByDeclaration(type.DeclId);

        //USAGE REFERENCE
        if (type.Parent is UDeclType parent)
        {
            typeRef = new(
                typeRef.Namespace,
                typeRef.Name,
                _module,
                typeRef.Scope,
                typeRef.IsValueType
            )
            {
                DeclaringType = ResolveDeclaredType(parent)
            };
        }
        if (type.Args.Length > 0)
        {
            var genRef = new GenericInstanceType(typeRef);
            foreach (var arg in type.Args)
                genRef.GenericArguments.Add(ResolveUsageType(arg));

            typeRef = genRef;
        }
        return typeRef;
    }

    private VInfo ReadAt(GlobalId symId)
    => ((VAssemblyEdit)Project.AssemblyAt(symId.AssemblyId)).ReadAt(symId.LocalId);
    private T ReadAt<T>(GlobalId symId) where T : VInfo
    => ((VAssemblyEdit)Project.AssemblyAt(symId.AssemblyId)).ReadAt<T>(symId.LocalId);
    private TypeDefinition TypeDefinitionAt(GlobalId symId)
    => ((VAssemblyEdit)Project.AssemblyAt(symId.AssemblyId)).EditAt<VTypeEdit>(symId.LocalId).Definition;
}