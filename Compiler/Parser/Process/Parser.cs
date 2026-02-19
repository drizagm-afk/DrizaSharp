using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== PARSING PROCESS =====
    private DzProject Project = null!;
    private int phase = 0;

    public void ParseProject(DzProject project)
    => ParseProject(project, 0);
    public void ParseProject(DzProject project, byte phase)
    {
        Project = project;
        this.phase = phase;

        foreach (var file in Project.Files)
            RegisterFileSites(file);

        ParseSites(this);
        _sites.Clear();
    }

    //FIND SITES
    public List<ParserSite> Sites => _sites;
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
        if (root.IsFlat() && IsNodeSite(root, phase))
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
                    if (IsNodeSite(child, phase))
                        action(i);
                }
                else Nodes.Push(i);
            });
        }
    }
    private static bool IsNodeSite(in TASTNode node, int phase)
    => node.Args.OutPhase == phase;

    //PARSE SITES
    private ParserSite Site = null!;
    private TAST TAST => Project.Files[Site.FileId].TAST;

    //YOU CAN CREATE YOUR OWN PARSE PHASES, THIS WILL CHANGE ACCORDINGLY TO WHAT YOU GIVE IT
    internal void ParseSites(ParserProcess process)
    {
        foreach (var site in process.Sites)
            process.EvalMatch(site);
        //foreach (var site in _sites) EvalValidate(project, phaseCode, site);
    }
}

public partial class ParserSite(int siteId, int fileId, int rootId)
{
    public readonly int SiteId = siteId;
    public readonly int FileId = fileId;
    public readonly int RootId = rootId;
}