using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler;

public enum DzProjectType { SINGLE_FILE, MULTI_FILE }
public class DzProject
{
    public int Id { get; }
    public string Path { get; }
    public DzProjectType Type { get; }

    internal DzProject(int id, string path, DzProjectType type)
    {
        Id = id;
        Path = path;
        Type = type;
    }

    public VIR VIR { get; } = new();
    public VAssembly AssemblyAt(int assemblyId)
    {
        if (assemblyId < 0)
            return VIR;

        return CompilationContext.AssemblyAt(assemblyId);
    }

    public ArrayView<DzModule> Modules { get; internal set; }
    public ArrayView<DzFile> Files { get; internal set; }

    public readonly ProjectDiagnostics LoaderDiagnostics = new();
    public readonly ProjectDiagnostics ParserDiagnostics = new();
    public readonly ProjectDiagnostics LowererDiagnostics = new();
}
public class DzModule
{
    public int Id { get; }
    public int NspaceId { get; }

    internal DzModule(int id, int nspaceId)
    {
        Id = id;
        NspaceId = nspaceId;
    }

    public ArrayView<GlobalId> Dependencies { get; internal set; }
}
public class DzFile
{
    public int Id { get; }
    public string Path { get; }

    public int ModuleId { get; }

    internal DzFile(int id, string path, int moduleId, SourceText source)
    {
        Id = id;
        Path = path;
        ModuleId = moduleId;

        Source = source;
        TAST = new(source);
        TASI = new();
    }

    public SourceText Source { get; }
    public TAST TAST { get; }
    public TASI TASI { get; }

    public readonly FileDiagnostics<RuleId> LexerDiagnostics = new();
    public readonly FileDiagnostics<int> ParserDiagnostics = new();
    public readonly FileDiagnostics<int> LowererDiagnostics = new();
}

public readonly struct FileNodeId(int fileId, int nodeId)
{
    public readonly int FileId = fileId;
    public readonly int NodeId = nodeId;
}