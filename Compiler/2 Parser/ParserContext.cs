using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

public interface Context
{
    public byte PhaseCode { get; }

    public R GetRule<R>() where R : Rule;
    public int GetRuleId<R>() where R : Rule;
    public R GetRule<R>(int id) where R : Rule;
    public Rule GetRule(int id);

    public C GetRuleClass<C>() where C : RuleClass;
    public int GetRuleClassId<C>() where C : RuleClass;
    public C GetRuleClass<C>(int id) where C : RuleClass;
    public RuleClass GetRuleClass(int id);

    //**TAST NODES**
    public TASTNode NodeAt(int nodeId);

    public void Children(Action<int> action);
    public void Children(int nodeId, Action<int> action);

    //**TAST TOKENS**
    public Token TokenAt(int tokenId);

    public ReadOnlySpan<char> GetTextSpan(int tokenId);
    public string GetText(int tokenId);
}
public partial class ParserProcess : Context
{
    //**PARSING PROCESS**
    public byte PhaseCode => _phaseCode;

    public R GetRule<R>() where R : Rule
    => ParserManager.GetRule<R>();
    public int GetRuleId<R>() where R : Rule
    => ParserManager.GetRuleId<R>();
    public R GetRule<R>(int id) where R : Rule
    => ParserManager.GetRule<R>(id);
    public Rule GetRule(int id)
    => ParserManager.GetRule(id);

    public C GetRuleClass<C>() where C : RuleClass
    => ParserManager.GetRuleClass<C>();
    public C GetRuleClass<C>(int id) where C : RuleClass
    => ParserManager.GetRuleClass<C>(id);
    public RuleClass GetRuleClass(int id)
    => ParserManager.GetRuleClass(id);
    public int GetRuleClassId<C>() where C : RuleClass
    => ParserManager.GetRuleClassId<C>();

    //**TAST NODES**
    public TASTNode NodeAt(int nodeId) => TAST.NodeAt(nodeId);

    public void Children(Action<int> action) => TAST.Children(action);
    public void Children(int nodeId, Action<int> action) => TAST.Children(nodeId, action);

    //**TAST TOKENS**
    public Token TokenAt(int tokenId) => TAST.TokenAt(tokenId);

    public ReadOnlySpan<char> GetTextSpan(int tokenId) => TAST.GetTextSpan(tokenId);
    public string GetText(int tokenId) => TAST.GetText(tokenId);
}