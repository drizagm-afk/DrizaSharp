using System.Collections.Immutable;
using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler;

public enum DzProjectType { SINGLE_FILE, MULTI_FILE }
public class DzProject
{
    public readonly int Id;
    public readonly string Path;
    public readonly DzProjectType Type;

    internal DzProject(int id, string path, DzProjectType type)
    {
        Id = id;
        Path = path;
        Type = type;
    }

    public VIR Virtual = new();

    public ImmutableArray<DzModule> Modules { get; internal set; }
    public ImmutableArray<DzFile> Files { get; internal set; }

    public readonly ProjectDiagnostics LoaderDiagnostics = new();
    public readonly ProjectDiagnostics ParserDiagnostics = new();
    public readonly ProjectDiagnostics LowererDiagnostics = new();
}
public class DzModule
{
    public readonly int Id;
    public readonly int NspaceId;

    internal DzModule(int id, int nspaceId)
    {
        Id = id;
        NspaceId = nspaceId;
    }

    public ImmutableArray<GlobalId> Dependencies { get; internal set; }
}
public class DzFile
{
    public readonly int Id;
    public readonly string Path;

    public readonly int ModuleId;

    internal DzFile(int id, string path, int moduleId, SourceText source)
    {
        Id = id;
        Path = path;
        ModuleId = moduleId;

        Source = source;
        TAST = new(source);
        TASI = new();
    }

    public readonly SourceText Source;
    public readonly TAST TAST;
    public readonly TASI TASI;

    public readonly FileDiagnostics<RuleId> LexerDiagnostics = new();
    public readonly FileDiagnostics<int> ParserDiagnostics = new();
    public readonly FileDiagnostics<int> LowererDiagnostics = new();
}

public readonly struct FileNodeId(int fileId, int nodeId)
{
    public readonly int FileId = fileId;
    public readonly int NodeId = nodeId;
}