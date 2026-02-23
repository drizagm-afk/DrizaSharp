using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Text;

namespace DrzSharp.Compiler.Project;

public class DzProject(string path, byte type)
{
    public readonly string Path = path;
    public readonly byte Type = type;
    public readonly VirtualWorld VWorld = new();

    /*FILES*/
    public readonly List<DzFile> Files = [];
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