using System.Collections.Immutable;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface MutateContext : Context
{
    //TOKENS
    public void AddToken(int type, string content);
    public void AddToken(int type, string content, SourceSlice source);
    public void AddToken(int type);
    public void AddToken(int type, SourceSlice source);

    //NODE TOKENS
    public void AddNodeTokens(RuleInstance inst)
    => AddNodeTokens(inst.NodeId);
    public void AddNodeTokens(int nodeId);
    public void AddNodeTokens(RuleInstance inst, SourceSlice source)
    => AddNodeTokens(inst.NodeId, source);
    public void AddNodeTokens(int nodeId, SourceSlice source);

    //EVALUATION
    public void AddEvalRule<R>() where R : Rule;
    public void AddEvalRuleClass<C>() where C : RuleClass;

    //APPLY MUTATION
    public void Rewrite();
    public void Append(int nodeId = -1);
}
public interface SemanticMutateContext : MutateContext, SemanticView { }

public partial class ParserProcess : SemanticMutateContext
{
    private int _tokenCount;
    public void AddToken(int type, string content)
    => AddToken(type, content, TAST.SourceSlice(RuleInst!.NodeId));
    public void AddToken(int type, string content, SourceSlice source)
    => TAST.NewToken(type, source.Start, source.Length, content);
    public void AddToken(int type)
    => AddToken(type, TAST.SourceSlice(RuleInst!.NodeId));
    public void AddToken(int type, SourceSlice source)
    => TAST.NewToken(type, source.Start, source.Length);

    private ImmutableArray<int>.Builder _tokenNodes = ImmutableArray.CreateBuilder<int>();
    public void AddNodeTokens(int nodeId)
    => AddNodeTokens(nodeId, TAST.SourceSlice(RuleInst!.NodeId));
    public void AddNodeTokens(int nodeId, SourceSlice source)
    {
        TAST.NewToken(Tokens.NULL_ID, source.Start, source.Length);
        _tokenNodes.Add(nodeId);
    }

    private ImmutableArray<RuleId>.Builder _evalRules = ImmutableArray.CreateBuilder<RuleId>();
    public void AddEvalRule<R>() where R : Rule
    => AddEvalRule(Project.GetRuleId<R>());
    public void AddEvalRuleClass<C>() where C : RuleClass
    => AddEvalRule(Project.GetRuleClassId<C>());
    private void AddEvalRule(RuleId rule)
    => _evalRules.Add(rule);

    //APPLY
    public void Rewrite()
    {
        var nodeId = RuleInst!.NodeId;

        TAST.Rewrite(nodeId, new(_tokenCount, TAST.TokenCount - _tokenCount), _tokenNodes.MoveToImmutable());
        TAST.UpdateLinearity(nodeId);

        ApplyMutate(nodeId, _evalRules.MoveToImmutable());
        RuleInst.Rewrite(this);
        ApplyRecompile(nodeId, false);

        _tokenCount = TAST.TokenCount;
    }
    public void Append(int nodeId = -1)
    {
        if (nodeId < 0)
            nodeId = RuleInst!.NodeId;

        var appendId = TAST.Append(nodeId, new(_tokenCount, TAST.TokenCount - _tokenCount), _tokenNodes.MoveToImmutable());
        TAST.UpdateLinearity(appendId);

        ApplyMutate(appendId, _evalRules.MoveToImmutable());
        if (TAST.TryGetApplyRule(nodeId, out var inst))
            inst.Append(this, appendId);
        ApplyRecompile(appendId, true);

        _tokenCount = TAST.TokenCount;
    }
}