using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Parser;

public partial class ParserProcess
{
    //===== VALIDATING MEMOIZATION =====
    private readonly HashSet<int> _validatedNodes = [];
    private bool IsValidated(int nodeId)
    => !_validatedNodes.Add(nodeId);

    //===== EXECUTE VALIDATING =====
    public void Validate(DzFile file)
    {
        File = file;
        Validate(TAST.Root);
        
        //END
        _validatedNodes.Clear();
        _scope.Clear();
        _scopeFrames.Clear();
    }
    private void Validate(in TASTNode node)
    {
        if (IsValidated(node.Id)) return;

        //VALIDATE
        if (TAST.InfoAt(node.Id).IsScoped) EnterScope();

        if (TAST.TryGetApplyRule(node.Id, out var inst))
        {
            RuleInst = inst;
            try { inst.Validate(this); }
            catch (Exception ex)
            {
                Diagnostics.ReportInvalid(
                    TAST.SourceSlice(node), 
                    ParserManager.GetRuleName(inst.RuleId), 
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

        if (TAST.InfoAt(node.Id).IsScoped) ExitScope();
    }
}