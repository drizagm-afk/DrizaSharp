using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    private void DebugVIR()
    => DebugVirtual(Project.VIR);
    private void DebugVirtual(VAssembly asm)
    {
        VIR? vir = asm as VIR;

        PrintSectionHeader("VIRTUAL");
        if (vir is not null)
            WriteLine(">> VIR (Virtual Intermediate Representation): ");
        else
            WriteLine($">> {asm.Name}: ");

        //LOOP
        bool[] debuggedNodes = new bool[asm.NodeCount];
        void debug(int nodeId, int tabs)
        => debugNode(asm.NodeAt(nodeId), tabs);
        void debugNode(in VNode node, int tabs)
        {
            var info = asm.ReadAt(node.Id);
            if (vir is null && info is VMember minfo && minfo.IsCompilerGenerated)
                return;

            if (debuggedNodes[node.Id])
                return;

            debuggedNodes[node.Id] = true;

            //HELPERS
            static string generics(VInfo info)
            {
                if (info is IGeneric gen && gen.GenericArity > 0)
                {
                    string[] genParams = new string[gen.GenericArity];
                    for (int i = 0; i < gen.GenericArity; i++)
                        genParams[i] = gen.GenericParams[i].Name;

                    return $"<{string.Join(", ", genParams)}>";
                }
                return "";
            }
            string parameters(VMethodMember method)
            {
                var paramCount = method.Params.Count;
                string[] methodParams = new string[paramCount];
                for (int i = 0; i < paramCount; i++)
                {
                    var param = method.Params[i];
                    methodParams[i] += $"{param.Name}: {UsageToString(param.Type)}";
                }
                return $"({string.Join(", ", methodParams)})";
            }

            //LOGIC
            string header;
            switch (info)
            {
                case VNspace nspace:
                    header = nspace.Name;
                    break;
                case VObject obj:
                    header = $"{obj.Name}{generics(obj)}";
                    break;
                case VStruct value:
                    header = $"{value.Name}{generics(value)}";
                    break;
                case VInterface inter:
                    header = $"{inter.Name}{generics(inter)}";
                    break;
                //FIELDS
                case VField field:
                    header = $"{field.Name}: {UsageToString(field.Type)}";
                    break;
                //PROPERTIES
                case VProperty property:
                    bool set = property.HasSetter();
                    bool get = property.HasGetter();

                    header = $"{property.Name}: {UsageToString(property.Type)} {{ {"set".If(set)}{", ".If(set && get)}{"get".If(get)} }}";
                    break;
                //METHODS
                case VMethod method:
                    header = $"{method.Name}{generics(method)}{parameters(method)}: {UsageToString(method.ReturnType)}";
                    break;
                case VCtor ctor:
                    header = $"{parameters(ctor)}";
                    break;
                case VAccessor accessor:
                    header = $"";
                    break;
                default:
                    return;
            }

            string kind = node.Id == 0 ? "GLOBAL NSPACE" : node.Kind switch
            {
                VKind.Nspace => "NSPACE",

                VKind.Object => "OBJECT",
                VKind.Struct => "STRUCT",
                VKind.Interface => "INTERFACE",

                VKind.Field => "FIELD",
                VKind.Property => "PROPERTY",

                VKind.Method => "METHOD",
                VKind.Ctor => "CTOR",
                VKind.Accessor => "ACCESSOR",
                _ => "NON-SUPPORTED"
            };

            if (node.Kind != VKind.Nspace && vir is VIR VIR && vir.TryGetSourceNode(node.Id, out var sourceId))
                PrintGConn($"[{kind}] {header} from <{sourceId.FileId:D3} | {sourceId.NodeId:D3}>", tabs);
            else
                PrintGConn($"[{kind}] {header}", tabs);

            //SPECIAL DEBUGGING
            if (info is VPropertyMember prop)
            {
                if (prop.HasSetter())
                    debug(prop.Setter, tabs + 1);
                if (prop.HasGetter())
                    debug(prop.Getter, tabs + 1);
            }

            //DEBUG CHILDREN
            int childId = node.FirstChildId;
            while (childId >= 0)
            {
                ref readonly var child = ref asm.NodeAt(childId);
                debugNode(child, tabs + 1);

                childId = child.NextSiblingId;
            }
        }
        debug(VAssemblyEdit.GlobalNspaceId, 0);
    }

    string OuterToString(VAssembly asm, int id)
    {
        var outerId = asm.NodeAt(id).ParentId;
        if (outerId > 0)
            return $"{OuterToString(asm, outerId)}{asm.ReadAt(outerId).Name}.";

        return "";
    }
    private string UsageToString(UType utype)
    {
        switch (utype)
        {
            case UDeclType declType:
                var asm = Project.AssemblyAt(declType.DeclId.AssemblyId);
                var name = asm.ReadAt(declType.DeclId.LocalId).Name;

                IEnumerable<string> debugArgs()
                {
                    foreach (var arg in declType.Args)
                        yield return UsageToString(arg);
                }
                if (declType.Args.Length > 0)
                    name += $"<{string.Join(", ", debugArgs())}>";

                if (declType.Parent is UType parent)
                    return $"{UsageToString(parent)}.{name}";

                return $"{OuterToString(asm, declType.DeclId.LocalId)}{name}";
            case UGenType genType:
                return (InfoAt(genType.DeclId) as IGeneric)!.GenericParams[genType.ParamId].Name;
            case UArrayType arrayType:
                return $"{UsageToString(arrayType.Type)}[{",".Repeat(arrayType.Rank)}]";
            case UAddressType addressType:
                return $"{UsageToString(addressType.Type)}&";
            case UPointerType ptrType:
                return $"{UsageToString(ptrType.Type)}*";
            default:
                return "<INVALID>";
        }
    }
    private VInfo InfoAt(GlobalId globalId)
    => Project.AssemblyAt(globalId.AssemblyId).ReadAt(globalId.LocalId);
}