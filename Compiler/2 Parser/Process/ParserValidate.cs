using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== VALIDATING MEMOIZATION =====
    private readonly HashSet<int> _validatedNodes = [];
    private bool IsValidated(int nodeId)
    => !_validatedNodes.Add(nodeId);

    //===== EXECUTE VALIDATING =====
    public void Validate(ParserSite site)
    {
        Site = site;
        Validate(site.RootId);
        //END
        _validatedNodes.Clear();
        _scope.Clear();
        _scopeFrames.Clear();
    }
    private void Validate(in TASTNode node)
    {
        if (node.Args.OutCode != _phaseCode || IsValidated(node.Id)) return;

        //VALIDATE
        if (node.Args.IsScoped) EnterScope();

        if (Site._ruleAppliance.TryGetValue(node.Id, out var inst))
        {
            RuleInst = inst;
            try { inst.Validate(this); }
            catch (Exception ex)
            {
                Diagnostics.ReportInvalid(
                    TAST.NodeSourceSlice(node), 
                    ParserManager.GetRule(inst.RuleId).RuleName, 
                    ex.Message
                );
            }
        }

        var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            Validate(child);
            childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
        }

        if (node.Args.IsScoped) ExitScope();
    }
}