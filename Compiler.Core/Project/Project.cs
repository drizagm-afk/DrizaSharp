using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Core;

public class DzProject(string path, byte type)
{
    public readonly string Path = path;
    public readonly byte Type = type;
    public readonly VirtualWorld VWorld = new();

    /*FILES*/
    public readonly List<DzFile> Files = [];
}
public class DzFile(int id, string path, StringSpan content)
{
    public readonly int Id = id;
    public readonly string Path = path;

    public readonly StringSpan Content = content;
    public readonly TAST TAST = new(content);
}