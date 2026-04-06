using DrzSharp.Compiler.Diagnostics;

namespace DrzSharp.Compiler.Loader;

public static class Manager
{
    //PROCESSES
    public static LoaderProcess NewProcess(string path) => new(path);
    public static void EndProcess(this LoaderProcess process) { }
}
public partial class LoaderProcess
{
    public DzProject Project { get; internal set; }
    private CompilationContext Context => CompilationContext.ContextAt(Project.Id);

    private ProjectDiagnostics GlobalDiagnostics => Project.LoaderDiagnostics;

    internal LoaderProcess(string path)
    {
        Project = new DzProject(CompilationContext.EnsureContext(path), path, GetProjectType(path));
    }
    private static DzProjectType GetProjectType(string path)
    {
        return Path.GetExtension(path) switch
        {
            ".dz" => DzProjectType.SINGLE_FILE,
            ".dzproj" => DzProjectType.MULTI_FILE,
            _ => throw new Exception("INVALID TARGET FILE")
        };
    }

    //>>>> PHASES <<<<
    public partial void Restore();
    public partial void Load();
}