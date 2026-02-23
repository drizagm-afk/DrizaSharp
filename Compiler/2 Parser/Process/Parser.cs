using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== PARSING PROCESS =====
    private DzProject Project = null!;
    private int _phaseCode = 0;

    public void ParseProject(DzProject project)
    => ParseProject(project, 0);
    public void ParseProject(DzProject project, byte phaseCode)
    {
        Project = project;
        _phaseCode = phaseCode;

        foreach (var file in Project.Files)
            RegisterFileSites(file);
        
        ParserManager._phases[phaseCode].phase(this);
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
        if (root.IsFlat() && IsNodeSite(root, _phaseCode))
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
                    if (IsNodeSite(child, _phaseCode))
                        action(i);
                }
                else Nodes.Push(i);
            });
        }
    }
    private static bool IsNodeSite(in TASTNode node, int phase)
    => node.Args.OutCode == phase;
}

public partial class ParserSite(int siteId, int fileId, int rootId)
{
    public readonly int SiteId = siteId;
    public readonly int FileId = fileId;
    public readonly int RootId = rootId;
}