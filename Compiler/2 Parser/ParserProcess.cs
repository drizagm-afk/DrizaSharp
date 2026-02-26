using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== PARSING PROCESS =====
    private DzProject Project = null!;
    private byte _phaseCode = 0;

    public void ParseProject(DzProject project)
    {
        for (byte i = 0; i < ParserManager._phases.Length; i++)
            ParseProject(project, i);
    }
    public void ParseProject(DzProject project, byte phaseCode)
    {
        Project = project;
        _phaseCode = phaseCode;

        foreach (var file in Project.Files)
            RegisterFileSites(file);

        ParserManager._phases[phaseCode].phase(this);

        Reset();
    }

    public void Reset()
    {
        Site = null!;
        _sites.Clear();
    }

    //FIND SITES
    private readonly List<ParserSite> _sites = [];

    private void RegisterFileSites(DzFile file)
    {
        PerFileSite(file, i => _sites.Add(new(_sites.Count, file.Id, i)));
    }
    private void PerFileSite(DzFile file, Action<int> action)
    {
        var TAST = file.TAST;
        var root = TAST.Root;

        Stack<int> Nodes = [];
        Nodes.Push(root.Id);

        //FLAT ROOT
        if (root.IsFlat() && IsNodeSite(TAST, root.Id, _phaseCode))
        {
            action(root.Id);
            return;
        }

        //FLAT CHILDREN
        while (Nodes.Count > 0)
        {
            TAST.Children(Nodes.Pop(), i =>
            {
                ref readonly var child = ref TAST.NodeAt(i);
                if (child.IsFlat())
                {
                    if (IsNodeSite(TAST, child.Id, _phaseCode))
                        action(i);
                }
                else Nodes.Push(i);
            });
        }
    }
    private static bool IsNodeSite(TAST TAST, int nodeId, int phase)
    => TAST.ArgsAt(nodeId).OutCode == phase;

    public void ForeachSite(Action<ParserSite> action)
    {
        foreach (var site in _sites)
            action(site);
    }

    //===== PARSER PHASE =====
    private ParserSite Site = null!;
    private DzFile File => Project.Files[Site.FileId];

    private GroupDiagnostics Diagnostics => File.Diagnostics.Parser;
    private TAST TAST => File.TAST;
    private TASI TASI => File.TASI;
}

public partial class ParserSite(int siteId, int fileId, int rootId)
{
    public readonly int SiteId = siteId;
    public readonly int FileId = fileId;
    public readonly int RootId = rootId;
}