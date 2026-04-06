using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //>>>> BIND PROJECT <<<<
    public partial void Bind()
    {
        foreach (var file in Project.Files)
            Bind(file);
        Mutate(Pass.Bind);
        foreach (var file in Project.Files)
            BindData(file);
        Mutate(Pass.BindData);
    }

    //>>>> BIND FILE <<<<
    private void Bind(DzFile file)
    {
        File = file;

        InitTagsMemory();
        Bind(TAST.Root);
        ClearTagsMemory();
    }
    private void BindData(DzFile file)
    {
        File = file;

        InitTagsMemory();
        BindData(TAST.Root);
        ClearTagsMemory();
    }

    private void Bind(in TASTNode node) { }
    private void BindData(in TASTNode node) { }
}