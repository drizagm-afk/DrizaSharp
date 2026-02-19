using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Evaluator;

public static class VirtualEval
{
    public static void LoadAssembly(VirtualWorld vworld, string path)
    {
        var vassembly = vworld.EnsureAssembly(path);

        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        var md = pe.GetMetadataReader();

        //TYPES
        foreach (var typeDefHandle in md.TypeDefinitions)
        {
            var typeDef = md.GetTypeDefinition(typeDefHandle);
            var typeDefName = md.GetString(typeDef.Name);
            if (typeDefName.StartsWith('<')) continue;

            (var nsName, var typeName) = EvalTypeDefinition(md, typeDef, typeDefName);

            var vns = vassembly.EnsureNamespace(nsName);
            var typeArgs = new TypeArgs(typeDef.Attributes, typeDef.GetGenericParameters().Count);
            var vtype = vns.AddChild(vns.NewInstance(InstanceKind.Type, typeName, typeArgs));

            //METHODS
            foreach (var methodHandle in typeDef.GetMethods())
            {
                var methodDef = md.GetMethodDefinition(methodHandle);
                var methodName = md.GetString(methodDef.Name);
                var methodArgs = new MethodArgs(methodDef.Attributes, methodDef.Signature, methodDef.GetGenericParameters().Count);
                vns.AddChild(vtype, vns.NewInstance(InstanceKind.Method, methodName, methodArgs));
            }

            //FIELDS
            foreach (var fieldHandle in typeDef.GetFields())
            {
                var fieldDef = md.GetFieldDefinition(fieldHandle);
                var fieldName = md.GetString(fieldDef.Name);
                var fieldArgs = new FieldArgs(fieldDef.Attributes, fieldDef.Signature);
                vns.AddChild(vtype, vns.NewInstance(InstanceKind.Field, fieldName, fieldArgs));
            }
        }
    }

    private static (string, string) EvalTypeDefinition(MetadataReader md, TypeDefinition typeDef, string typeDefName)
    {
        //EVALUATING NESTING
        if (typeDef.IsNested)
        {
            var handle = typeDef.GetDeclaringType();
            var declType = md.GetTypeDefinition(handle);
            (var ns, var fullDeclName) = EvalTypeDefinition(md, declType, md.GetString(declType.Name));
            return (ns, fullDeclName + "." + typeDefName);
        }

        //RETURNING DATA
        return (md.GetString(typeDef.Namespace), typeDefName);
    }
}