using System.Collections.Immutable;

namespace DrzSharp.Compiler.Loader;

public partial class LoaderProcess
{
    public partial void Load()
    {
        var builder = ImmutableArray.CreateBuilder<DzFile>();

        if (Project.Type == DzProjectType.SINGLE_FILE)
            builder.Add(LoadFile(builder.Count, Project.Path, 0));
        if (Project.Type == DzProjectType.MULTI_FILE)
            throw new Exception("MULTI-FILE PROJECTS ARE UNSUPPORTED YET");
        
        Project.Files = builder.ToImmutable();
    }
    private DzFile LoadFile(int id, string path, int moduleId)
    {
        //TAST INITIATION
        var content = new SourceText(File.ReadAllText(path));
        return new DzFile(id, path, moduleId, content);
    }
}