namespace DrzSharp.Compiler.Virtual;

public static class VirtualDebugger
{
    //DEBUG GRAPHS
    const string GRAPH_CONN = " ├─ ";
    const string GRAPH_TAB = " │  ";

    private static void PrintGConn(string cont, int tabs)
    {
        if (tabs == 0)
            Console.WriteLine(cont);
        else
        {
            Console.WriteLine(
                GRAPH_TAB.Repeat(tabs - 1) + GRAPH_CONN + cont
            );
        }
    }
    private static void PrintGTab(string cont, int tabs)
    => Console.WriteLine(GRAPH_TAB.Repeat(tabs) + cont);

    public static void Debug(VAssembly asm)
    {
        //PRINT GRAPH
        Console.WriteLine($"======= {asm.Name} =======");
        void debug(in VNode node, int tabs)
        {
            var info = asm.ReadInfoAt(node.Id);
            if (info is VMember minfo && minfo.IsCompilerGenerated)
                return;

            string header;
            if (info is VNspace nspace)
                header = nspace.Name;
            else if (info is VMember member)
            {
                header = member.Name;

                if (member is IReadGeneric gen && gen.GenericArity > 0)
                {
                    string[] genParams = new string[gen.GenericArity];
                    for (int i = 0; i < gen.GenericArity; i++)
                        genParams[i] = gen.GenericParams[i].Name;
                    
                    header += $"<{string.Join(", ", genParams)}>";
                }
            }
            else return;

            string kind = node.Kind switch
            {
                VKind.Nspace => "NSPACE",

                VKind.Type => "TYPE",
                VKind.Interface => "INTERFACE",

                VKind.Field => "FIELD",
                VKind.Property => "PROPERTY",

                VKind.Method => "METHOD",
                VKind.Ctor => "CTOR",
                VKind.Accessor => "ACCESSOR",
                _ => "NON-SUPPORTED"
            };

            PrintGConn($"[{kind}] {header}", tabs);

            //DEBUG CHILDREN
            int childId = node.FirstChildId;
            while (childId >= 0)
            {
                ref readonly var child = ref asm.NodeAt(childId);
                debug(child, tabs + 1);

                childId = child.NextSiblingId;
            }
        }
        debug(asm.NodeAt(VAssembly.GlobalNspaceId), 0);
    }
}