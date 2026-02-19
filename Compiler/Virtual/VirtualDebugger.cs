namespace DrzSharp.Compiler.Virtual;

public static class VirtualDebugger
{
    public static void Debug(VirtualWorld vworld)
    {
        foreach(var assembly in vworld.Assemblies)
        {
            DebugAssembly(assembly.Value, assembly.Key);
        }
    }
    public static void DebugAssembly(VirtualAssembly vassembly, string assemblyName = "")
    {
        foreach(var ns in vassembly.Namespaces)
        {
            Console.WriteLine($"===== ${assemblyName}:${ns.Key} =====");
            DebugTree(ns.Value);
        }
    }
    public static void DebugTree(VirtualTree vtree)
    {
        DebugTree(vtree, VirtualTree.RootId);
    }

    public static void DebugTree(VirtualTree tree, int nodeId, string nameTrail = "")
    {
        //DEBUG SELF
        var node = tree.NodeAt(nodeId);

        var inst = tree.InstanceAt(node.InstanceId);
        nameTrail += "." + inst.Name;
        Console.WriteLine($"// ${nameTrail} //");
        Console.WriteLine(
            inst.Kind switch
            {
                InstanceKind.Type => "Type",
                InstanceKind.Method => "Method",
                InstanceKind.Field => "Field",
                _ => "DEFAULT",
            }
        );

        //DEBUG CHILDREN
        foreach (var childId in tree.Children(nodeId))
        {
            DebugTree(tree, childId, nameTrail);
        }
    }
}