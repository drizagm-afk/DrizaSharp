using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> MATCH PROJECT <<<<
    public partial void Match()
    {
        foreach (var file in Project.Files)
            Match(file);
        Mutate(Pass.Build);
    }

    //>>>> MATCH FILE <<<<
    public void Match(DzFile file)
    {
        File = file;
        Match(TAST.Root);
    }
}