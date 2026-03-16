using Mono.Cecil;
using DrzSharp.Compiler.Virtual;
using System.Collections.Immutable;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler.Evaluator;

public partial class EvalProcess
{
    private VirtualWorld VWorld => Project.VWorld;

    //UTILS
    private VNspace NspaceOf(VAssembly virtAsm, string nspaceFullName)
    {
        VNspace virtNspace = VWorld.EditInfoAt<VNspace>(virtAsm.GlobalNspace);
        if (nspaceFullName != string.Empty)
        {
            foreach (var nspaceName in nspaceFullName.Split('.'))
                virtNspace = VWorld.EnsureNspace(virtNspace.Id, nspaceName);
        }
        return virtNspace;
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

    //>>>> BINDING <<<<
    private int BindAssembly(AssemblyDefinition asm)
    {
        var virtAsm = VWorld.AddAssembly();
        Project._assemblyByName[asm.Name.FullName] = virtAsm.Hash;

        virtAsm.Definition = asm;

        //BIND
        foreach (var type in asm.MainModule.Types)
            BindType(NspaceOf(virtAsm, type.Namespace).Id, type);

        return virtAsm.Hash;
    }

    private static bool IsTypeGenerated(VTypeBase virtType, TypeDefinition type)
    {
        virtType.Definition = type;
        return virtType.IsCompilerGenerated = IsCompilerGenerated(type.Name);
    }
    private void BindMembers(VComposableType virtType)
    {
        var type = virtType.Definition;

        //**FIELDS**
        foreach (var field in type.Fields)
            BindField(virtType.Id, field);

        //**PROPERTIES**
        foreach (var prop in type.Properties)
            BindProperty(virtType.Id, prop);

        //**METHODS**
        foreach (var method in type.Methods)
            BindMethod(virtType.Id, method);
    }
    private void BindType(int outerId, TypeDefinition type)
    {
        var name = type.Name;
        //===== SPECIAL TYPES =====
        if (type.IsInterface)
        {
            BindInterface(outerId, type);
            return;
        }

        //===== BASE TYPE =====
        var virtType = VWorld.AddType(outerId, GenericNameOf(name));

        //**NESTED TYPES**
        foreach (var nestedType in type.NestedTypes)
            BindType(virtType.Id, nestedType);

        if (IsTypeGenerated(virtType, type))
            return;

        BindMembers(virtType);
    }
    private void BindInterface(int outerId, TypeDefinition type)
    {
        var virtType = VWorld.AddInterface(outerId, GenericNameOf(type.Name));

        if (IsTypeGenerated(virtType, type))
            return;

        BindMembers(virtType);
    }

    private static void BindFieldBase(VFieldBase virtField, FieldDefinition field)
    {
        virtField.Definition = field;
    }
    private void BindField(int typeId, FieldDefinition field)
    {
        var name = field.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL FIELDS =====

        //===== BASE FIELD =====
        var virtField = VWorld.AddField(typeId, name);
        BindFieldBase(virtField, field);
    }

    private void BindPropertyBase(int typeId, VPropertyBase virtProperty, PropertyDefinition property)
    {
        virtProperty.Definition = property;

        //BIND ACCESSORS
        int bind(MethodDefinition method)
        => BindAccessor(typeId, virtProperty.Id, method);

        if (property.GetMethod is not null)
            virtProperty.Getter = bind(property.GetMethod);
        if (property.SetMethod is not null)
            virtProperty.Setter = bind(property.SetMethod);
    }
    private void BindProperty(int typeId, PropertyDefinition property)
    {
        var name = property.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL PROPERTIES =====

        //===== BASE PROPERTY =====
        var virtProperty = VWorld.AddProperty(typeId, name);
        BindPropertyBase(typeId, virtProperty, property);
    }

    private static void BindMethodBase(VMethodBase virtMethod, MethodDefinition method)
    {
        virtMethod.Definition = method;
    }
    private void BindMethod(int typeId, MethodDefinition method)
    {
        var name = method.Name;
        if (IsCompilerGenerated(name))
            return;

        //===== SPECIAL METHODS =====
        if (method.IsConstructor)
        {
            BindCtor(typeId, method);
            return;
        }
        else if (method.IsGetter || method.IsSetter)
            return;

        //===== BASE METHOD =====
        var virtMethod = VWorld.AddMethod(typeId, GenericNameOf(name));
        BindMethodBase(virtMethod, method);
    }
    private void BindCtor(int typeId, MethodDefinition method)
    {
        var virtCtor = VWorld.AddCtor(typeId);
        BindMethodBase(virtCtor, method);
    }
    private int BindAccessor(int typeId, int sourceId, MethodDefinition method)
    {
        var virtAccessor = VWorld.AddAccessor(typeId, sourceId, method.Name);
        BindMethodBase(virtAccessor, method);

        return virtAccessor.Id;
    }

    //>>>> LOADING <<<<
    private void LoadAssemblies()
    {
        for (int hash = 0; hash < VWorld.Assemblies.Count; hash++)
        {
            var virtAsm = VWorld.EditAssembly(hash);
            var gNspace = VWorld.ReadInfoAt<IVReadOnlyNspace>(virtAsm.GlobalNspace);

            LoadNspace(gNspace);
        }
    }

    private void LoadContainedTypes(IVReadOnlyTypeContainer container)
    {
        foreach (var (_, typeId) in container.Types)
        {
            var kind = VWorld.KindOf(typeId);
            if (kind == VKind.Type)
                LoadType(VWorld.EditInfoAt<VType>(typeId));
            else if (kind == VKind.Interface)
                LoadInterface(VWorld.EditInfoAt<VInterface>(typeId));
        }
    }
    private void LoadNspace(IVReadOnlyNspace nspace)
    {
        LoadContainedTypes(nspace);

        foreach (var (_, nspaceId) in nspace.Nspaces)
            LoadNspace(VWorld.ReadInfoAt<IVReadOnlyNspace>(nspaceId));
    }

    private void LoadGenericParams(IGeneric virt, Collection<GenericParameter> generics, VMethod? virtMethod = null)
    {
        //GENERIC PARAMETERS
        var genBuilder = ImmutableArray.CreateBuilder<VGenericParam>(generics.Count);
        foreach (var genParam in generics)
        {
            //CONSTRAINTS
            var constBuilder = ImmutableArray.CreateBuilder<UType>(genParam.Constraints.Count);
            foreach (var constraint in genParam.Constraints)
            {
                constBuilder.Add(
                    ResolveReference(constraint.ConstraintType, virtMethod)
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
        virt.GenericParams = genBuilder.MoveToImmutable();
    }
    private TypeDefinition LoadBaseType(VTypeBase virtType)
    {
        var type = virtType.Definition;

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

        virtType.Visibility = vis;

        return type;
    }
    private void LoadComposableType(VComposableType virtType)
    {
        var type = virtType.Definition;

        //GENERICS
        LoadGenericParams(virtType, type.GenericParameters);

        //INTERFACES
        var interBuilder = ImmutableArray.CreateBuilder<UTypeDef>(type.Interfaces.Count);
        foreach (var inter in type.Interfaces)
            interBuilder.Add(ResolveDefinition(inter.InterfaceType));

        virtType.Interfaces = interBuilder.MoveToImmutable();

        //MEMBERS
        foreach (var ctorId in virtType.Ctors)
            LoadCtor(VWorld.EditInfoAt<VCtor>(ctorId));
        foreach (var (_, memberIds) in virtType.Members)
        {
            foreach (var memberId in memberIds)
            {
                var kind = VWorld.KindOf(memberId);
                //FIELDS
                if (kind == VKind.Field)
                    LoadField(VWorld.EditInfoAt<VField>(memberId));
                //PROPERTIES
                else if (kind == VKind.Property)
                    LoadProperty(VWorld.EditInfoAt<VProperty>(memberId));
                //METHODS
                else if (kind == VKind.Accessor)
                    LoadAccessor(VWorld.EditInfoAt<VAccessor>(memberId));
            }
        }
        foreach (var (_, genericMemberIds) in virtType.GenericMembers)
        {
            foreach (var genericMemberId in genericMemberIds)
            {
                var kind = VWorld.KindOf(genericMemberId);
                if (kind == VKind.Method)
                    LoadMethod(VWorld.EditInfoAt<VMethod>(genericMemberId));
            }
        }
    }
    private void LoadType(VType virtType)
    {
        var type = LoadBaseType(virtType);

        //BASE
        if (type.BaseType != null)
            virtType.Base = ResolveReference(type.BaseType);

        //LAYOUT
        var lay = VTypeLayout.AUTO;
        if (type.IsSequentialLayout)
            lay = VTypeLayout.SEQUENTIAL;
        else if (type.IsExplicitLayout)
            lay = VTypeLayout.EXPLICIT;

        virtType.Layout = lay;

        //ATTRIBUTES
        virtType.IsAbstract = type.IsAbstract;
        virtType.IsSealed = type.IsSealed;

        LoadComposableType(virtType);
        LoadContainedTypes(virtType);
    }
    private void LoadInterface(VInterface virtInterface)
    {
        LoadBaseType(virtInterface);
        LoadComposableType(virtInterface);
    }

    private FieldDefinition LoadBaseField(VFieldBase virtField)
    {
        var field = virtField.Definition;

        //TYPE
        virtField.Type = ResolveReference(field.FieldType);

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

        virtField.Visibility = vis;

        return field;
    }
    private void LoadField(VField virtField)
    {
        var field = LoadBaseField(virtField);

        //ATTRIBUTES
        virtField.IsStatic = field.IsStatic;
    }

    private PropertyDefinition LoadBaseProperty(VPropertyBase virtProperty)
    {
        var property = virtProperty.Definition;

        //TYPE
        virtProperty.Type = ResolveReference(property.PropertyType);

        return property;
    }
    private void LoadProperty(VProperty virtProperty)
    {
        LoadBaseProperty(virtProperty);
    }

    private MethodDefinition LoadBaseMethod(VMethodBase virtMethod)
    {
        var method = virtMethod.Definition;

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
                param.Name, ResolveReference(param.ParameterType, virtMethod), mod
            ));
        }
        virtMethod.Params = paramBuilder.MoveToImmutable();

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

        virtMethod.Visibility = vis;

        return method;
    }
    private void LoadReturnType<T>(T virtMethod) where T : VMethodBase, IReturnable
    {
        var method = virtMethod.Definition;

        //RETURN TYPE
        var ret = virtMethod.Definition.ReturnType;
        if (ret.FullName == "System.Void")
            virtMethod.ReturnType = UVoidType.Type;
        else
            virtMethod.ReturnType = ResolveReference(ret, virtMethod);
    }
    private void LoadMethod(VMethod virtMethod)
    {
        var method = LoadBaseMethod(virtMethod);

        //GENERICS
        LoadGenericParams(virtMethod, method.GenericParameters, virtMethod);

        //RETURN TYPE
        LoadReturnType(virtMethod);

        //ATTRIBUTES
        virtMethod.IsStatic = method.IsStatic;
        virtMethod.IsAbstract = method.IsAbstract;
        virtMethod.IsVirtual = method.IsVirtual;
    }
    private void LoadCtor(VCtor virtCtor)
    {
        var method = LoadBaseMethod(virtCtor);

        //ATTRIBUTES
        virtCtor.IsStatic = method.IsStatic;
    }
    private void LoadAccessor(VAccessor virtAccessor)
    {
        var method = LoadBaseMethod(virtAccessor);

        //RETURN TYPE
        LoadReturnType(virtAccessor);

        //ATTRIBUTES
        virtAccessor.IsStatic = method.IsStatic;
        virtAccessor.IsAbstract = method.IsAbstract;
        virtAccessor.IsVirtual = method.IsVirtual;
    }

    //>>>> RESOLVE REFERENCES <<<<
    private UType ResolveReference(TypeReference typeRef, VMethodBase? virtMethod = null)
    {
        switch (typeRef)
        {
            //UN-SUPPORTED
            case OptionalModifierType modT:
                return ResolveReference(modT.ElementType, virtMethod);
            case RequiredModifierType modT:
                return ResolveReference(modT.ElementType, virtMethod);
            case PinnedType pinT:
                return ResolveReference(pinT.ElementType, virtMethod);
            case SentinelType senT:
                return ResolveReference(senT.ElementType, virtMethod);
            case FunctionPointerType:
                return VWorld.NewUPointerType(UVoidType.Type);
            //TYPES
            case ByReferenceType refT:
                return VWorld.NewUReferenceType(ResolveReference(refT.ElementType, virtMethod));
            case PointerType ptrT:
                return VWorld.NewUPointerType(ResolveReference(ptrT.ElementType, virtMethod));
            case ArrayType aryT:
                return VWorld.NewUArrayType(ResolveReference(aryT.ElementType, virtMethod), aryT.Rank);
            case GenericParameter genP:
                if (genP.Type == GenericParameterType.Method)
                {
                    if (virtMethod is null)
                        throw new Exception($"METHOD GENERIC PARAMETER OUTSIDE A METHOD CONTEXT: methoName={genP.FullName}");

                    return VWorld.NewUTypeParam(virtMethod!.Id, genP.Position);
                }
                else
                    return VWorld.NewUTypeParam(DefinitionToType(genP.DeclaringType).Id, genP.Position);
            default:
                return ResolveDefinition(typeRef, virtMethod);
        }
    }
    private UTypeDef ResolveDefinition(TypeReference typeRef, VMethodBase? virtMethod = null)
    {
        var decl = typeRef.DeclaringType;
        UTypeDef? parent = decl is not null ? ResolveDefinition(decl, virtMethod) : null;

        if (typeRef is GenericInstanceType genT)
        {
            var argsBuilder = ImmutableArray.CreateBuilder<UType>(genT.GenericArguments.Count);
            foreach (var genericArg in genT.GenericArguments)
                argsBuilder.Add(ResolveReference(genericArg, virtMethod));

            return VWorld.NewUTypeDef(DefinitionToType(genT.ElementType).Id, parent, argsBuilder.MoveToImmutable());
        }
        else
            return VWorld.NewUTypeDef(DefinitionToType(typeRef).Id, parent);
    }

    private VAssembly DefinitionToAssembly(TypeReference typeRef)
    {
        string? asmName = null;
        if (typeRef.Scope is AssemblyNameReference asmRef)
            asmName = asmRef.FullName;
        else if (typeRef.Module?.Assembly != null)
            asmName = typeRef.Module.Assembly.Name.FullName;

        int asmHash = Project.GetAssemblyByName(asmName);
        return VWorld.EditAssembly(asmHash);
    }
    private VTypeBase DefinitionToType(TypeReference typeRef)
    {
        VTypeBase ResolveOuterDef(TypeReference typeRef)
        {
            var declRef = typeRef.DeclaringType;
            if (declRef is not null)
            {
                var declDef = ResolveOuterDef(declRef);

                return VWorld.EditTypeBase(declDef.Id, GenericNameOf(typeRef.Name));
            }

            var virtAsm = DefinitionToAssembly(typeRef);
            var virtNspace = NspaceOf(virtAsm, typeRef.Namespace);

            return VWorld.EditTypeBase(virtNspace.Id, GenericNameOf(typeRef.Name));
        }
        return ResolveOuterDef(typeRef);
    }
}