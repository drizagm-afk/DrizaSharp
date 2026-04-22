using System.Collections.Immutable;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    //=======================
    //    DEFINE VIRTUAL
    //=======================
    private partial void DefineVirtual()
    => DefineContainedNspaces(VIR.ReadGlobalNspace());
    private void DefineContainedTypes(VInfo outer)
    {
        foreach (var type in VIR.EditTypes<VTypeEdit>(outer.Id))
            DefineType(outer, type);
    }
    private void DefineContainedNspaces(VInfo outer)
    {
        DefineContainedTypes(outer);

        foreach (var nspace in VIR.EditNspaces(outer.Id))
            DefineContainedNspaces(nspace);
    }

    //>>>> DEFINE TYPE <<<<
    private void DefineTypeMembers(VComposableTypeEdit type)
    {
        foreach (var field in VIR.EditMembers<VFieldMemberEdit>(type.Id))
            DefineField(type, field);
        foreach (var property in VIR.EditMembers<VPropertyMemberEdit>(type.Id))
            DefineProperty(type, property);
        foreach (var method in VIR.EditMembers<VMethodMemberEdit>(type.Id))
            DefineMethod(type, method);
    }
    private void DefineType(VInfo outer, VTypeEdit type)
    {
        static void define(ModuleDefinition module, VInfo outer, VTypeEdit type, string name, TypeAttributes attr, TypeReference? baseType)
        {
            var typeDef = type.Definition = new(
                outer is VNspace nspace ? nspace.FullName : "", name, attr, baseType
            );

            if (outer is VNspace)
                module.Types.Add(typeDef);
            else if (outer is VTypeEdit outerType)
                outerType.Definition.NestedTypes.Add(typeDef);
        }
        static void defineGenerics<T>(T type) where T : VTypeEdit, IGenericEdit
        {
            var def = type.Definition;
            foreach (var genParam in type.GenericParams)
                def.GenericParameters.Add(new(genParam.Name, def));
        }

        TypeAttributes attr = default;
        //VISIBILITY
        if (outer is VType)
            attr |= type.Visibility switch
            {
                VMemberVisibility.PUBLIC => TypeAttributes.NestedPublic,
                VMemberVisibility.ASSEMBLY => TypeAttributes.NestedAssembly,
                VMemberVisibility.PRIVATE => TypeAttributes.NestedPrivate,
                VMemberVisibility.FAMILY => TypeAttributes.NestedFamily,
                VMemberVisibility.FAMILY_OR_ASSEMBLY => TypeAttributes.NestedFamORAssem,
                VMemberVisibility.FAMILY_AND_ASSEMBLY => TypeAttributes.NestedFamANDAssem,
                _ => throw new Exception()
            };
        else
            attr |= type.Visibility switch
            {
                VMemberVisibility.PUBLIC => TypeAttributes.Public,
                VMemberVisibility.ASSEMBLY => TypeAttributes.NotPublic,
                _ => throw new Exception()
            };

        //LAYOUT
        if (type is ILayout ilayout)
        {
            attr |= ilayout.Layout switch
            {
                VTypeLayout.AUTO => TypeAttributes.AutoLayout,
                VTypeLayout.SEQUENTIAL => TypeAttributes.SequentialLayout,
                VTypeLayout.EXPLICIT => TypeAttributes.ExplicitLayout,
                _ => throw new Exception()
            };
        }

        //MODIFIERS
        if (type is IAbstract iabstract && iabstract.IsAbstract)
            attr |= TypeAttributes.Abstract;
        if (type is ISealed isealed && isealed.IsSealed)
            attr |= TypeAttributes.Sealed;

        //>>>> TYPES
        switch (type)
        {
            case VObjectEdit objType:
                define(_module, outer, type, NameOf(type),
                    attr | TypeAttributes.Class, null);
                defineGenerics(objType);
                break;
            case VStructEdit structType:
                define(_module, outer, type, NameOf(type),
                    attr | TypeAttributes.Sealed, REF_STRUCT);
                defineGenerics(structType);
                break;
            case VInterfaceEdit interfaceType:
                define(_module, outer, type, NameOf(type),
                    attr | TypeAttributes.Interface | TypeAttributes.Abstract, null);
                defineGenerics(interfaceType);
                break;
        }

        //>>>> TYPE MEMBERS
        if (type is VComposableTypeEdit composable)
            DefineTypeMembers(composable);
        if (VIR.IsTypeContainer(type.Id))
            DefineContainedTypes(type);
    }

    //>>>> DEFINE FIELD <<<<
    private void DefineField(VTypeEdit type, VFieldMemberEdit field)
    {
        static void define(VTypeEdit type, VFieldMemberEdit field, string name, FieldAttributes attr, TypeReference? fieldType)
        {
            var fieldDef = field.Definition = new(name, attr, fieldType);
            type.Definition.Fields.Add(fieldDef);
        }

        FieldAttributes attr = default;
        //VISIBILITY
        attr |= field.Visibility switch
        {
            VMemberVisibility.PUBLIC => FieldAttributes.Public,
            VMemberVisibility.ASSEMBLY => FieldAttributes.Assembly,
            VMemberVisibility.PRIVATE => FieldAttributes.Private,
            VMemberVisibility.FAMILY => FieldAttributes.Family,
            VMemberVisibility.FAMILY_OR_ASSEMBLY => FieldAttributes.FamORAssem,
            VMemberVisibility.FAMILY_AND_ASSEMBLY => FieldAttributes.FamANDAssem,
            _ => throw new Exception()
        };

        //MODIFIERS
        if (field is IStatic istatic && istatic.IsStatic)
            attr |= FieldAttributes.Static;

        //>>>> FIELD
        switch (field)
        {
            case VFieldEdit:
                define(type, field, NameOf(field), attr, null);
                break;
        }
    }

    //>>>> DEFINE PROPERTY <<<<
    private void DefineProperty(VTypeEdit type, VPropertyMemberEdit property)
    {
        static void define(VTypeEdit type, VPropertyMemberEdit property, string name, PropertyAttributes attr, TypeReference? propertyType)
        {
            var propertyDef = property.Definition = new(name, attr, propertyType);
            type.Definition.Properties.Add(propertyDef);
        }

        PropertyAttributes attr = default;

        //>>>> PROPERTY
        switch (property)
        {
            case VPropertyEdit:
                define(type, property, NameOf(property), attr, null);
                break;
        }
    }

    //>>>> DEFINE METHOD <<<<
    private void DefineMethod(VTypeEdit type, VMethodMemberEdit method)
    {
        static void define(VTypeEdit type, VMethodMemberEdit method, string name, MethodAttributes attr, TypeReference? retType)
        {
            var methodDef = method.Definition = new(name, attr, retType);
            type.Definition.Methods.Add(methodDef);
        }
        static void defineGenerics<T>(T method) where T : VMethodMemberEdit, IGenericEdit
        {
            var def = method.Definition;
            foreach (var genParam in method.GenericParams)
                def.GenericParameters.Add(new(genParam.Name, def));
        }

        MethodAttributes attr = default;
        //VISIBILITY
        attr |= method.Visibility switch
        {
            VMemberVisibility.PUBLIC => MethodAttributes.Public,
            VMemberVisibility.ASSEMBLY => MethodAttributes.Assembly,
            VMemberVisibility.PRIVATE => MethodAttributes.Private,
            VMemberVisibility.FAMILY => MethodAttributes.Family,
            VMemberVisibility.FAMILY_OR_ASSEMBLY => MethodAttributes.FamORAssem,
            VMemberVisibility.FAMILY_AND_ASSEMBLY => MethodAttributes.FamANDAssem,
            _ => throw new Exception()
        };

        //MODIFIERS
        if (method is IStatic istatic && istatic.IsStatic)
            attr |= MethodAttributes.Static;
        if (method is IAbstract iabstract && iabstract.IsAbstract)
            attr |= MethodAttributes.Abstract;
        if (method is IVirtual ivirtual && ivirtual.IsVirtual)
            attr |= MethodAttributes.Virtual;
        if (method is IFinal ifinal && ifinal.IsFinal)
            attr |= MethodAttributes.Final;

        //>>>> METHOD
        switch (method)
        {
            case VMethodEdit defMethod:
                define(type, method, NameOf(method), attr, null);
                defineGenerics(defMethod);
                break;
            case VCtor ctorMethod:
                define(type, method, ctorMethod.IsStatic ? ".cctor" : ".ctor",
                    attr | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, REF_VOID);
                break;
            case VAccessor accessMethod:
                define(type, method, NameOf(method),
                    attr | MethodAttributes.SpecialName, null);

                var property = VIR.EditAt<VPropertyEdit>(accessMethod.SourceId);
                if (accessMethod.Kind == VAccessorKind.Getter)
                    property.Definition.GetMethod = method.Definition;
                else
                    property.Definition.SetMethod = method.Definition;
                break;
        }
    }

    //=======================
    //     NAME RESOLVER
    //=======================
    private static string NameOf(VInfo info)
    {
        if (info is IGeneric gen && gen.GenericArity > 0)
            return $"{info.Name}`{gen.GenericArity}";

        return info.Name;
    }

    //=======================
    //     SYSTEM TYPES
    //=======================
    private TypeReference REF_OBJECT => ReferenceByDeclaration(CTX.TYPE_OBJECT);
    private TypeReference REF_STRUCT => ReferenceByDeclaration(CTX.TYPE_STRUCT);
    private TypeReference REF_VOID => ReferenceByDeclaration(CTX.TYPE_VOID);

    private readonly Dictionary<GlobalId, TypeReference> _refByDecl = [];
    private TypeReference ReferenceByDeclaration(VType type)
    => ReferenceByDeclaration(type.GlobalId, type);
    private TypeReference ReferenceByDeclaration(GlobalId declId, VType? type = null)
    {
        if (_refByDecl.TryGetValue(declId, out var typeRef))
            return typeRef;
        else
        {
            TypeDefinition typeDef = type switch
            {
                VTypeEdit edit => edit.Definition,
                _ => TypeDefinitionAt(declId)
            };
            return _refByDecl[declId] = declId.AssemblyId >= 0 ? _module.ImportReference(typeDef) : typeDef;
        }
    }
}