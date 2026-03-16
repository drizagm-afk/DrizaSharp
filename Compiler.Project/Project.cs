using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;
using DrzSharp.Compiler.Text;

namespace DrzSharp.Compiler.Project;

public enum DzProjectType { SINGLE_FILE, MULTI_FILE }
public class DzProject(string path, DzProjectType type)
{
    public readonly string Path = path;
    public readonly DzProjectType Type = type;
    public readonly VirtualWorld VWorld = new();

    /*FILES*/
    public readonly List<DzFile> Files = [];

    /*DEPENDENCIES*/
    internal int _assemblyCount = 0;
    internal readonly Dictionary<string, int> _assemblyByName = [];
    public IReadOnlyDictionary<string, int> AssemblyByName => _assemblyByName;

    public int GetAssemblyByName(string? assemblyName)
    {
        if (assemblyName is null)
            throw new Exception("ASSEMBLY NAME IS NULL");
        if (!_assemblyByName.TryGetValue(assemblyName, out var asmId))
            throw new Exception($"ASSEMBLY NAME IS NOT A DEPENDENCY: asmName={assemblyName}");

        return asmId;
    }
}
public class DzFile(int id, string path, SourceSpan content)
{
    public readonly int Id = id;
    public readonly string Path = path;

    public readonly SourceSpan Content = content;
    public readonly TAST TAST = new(content);
    public readonly TASI TASI = new();

    public readonly FileDiagnostics Diagnostics = new();
}