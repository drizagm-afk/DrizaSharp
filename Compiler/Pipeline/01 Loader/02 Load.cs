using System.Collections.Immutable;

namespace DrzSharp.Compiler.Loader;

public partial class LoaderProcess
{
    public partial void Load()
    {
        var builder = ImmutableArray.CreateBuilder<DzFile>();

        if (Project.Type == DzProjectType.SINGLE_FILE)
            builder.Add(LoadFile(Project.Path, 0));
        if (Project.Type == DzProjectType.MULTI_FILE)
            throw new Exception("MULTI-FILE PROJECTS ARE UNSUPPORTED YET");
        
        Project.Files = builder.MoveToImmutable();
    }
    private DzFile LoadFile(string path, int moduleId)
    {
        //TAST INITIATION
        var content = new SourceText(File.ReadAllText(path));
        return new DzFile(Project.Files.Length, path, moduleId, content);
    }
}