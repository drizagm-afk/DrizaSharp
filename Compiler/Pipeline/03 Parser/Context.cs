using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface Context
{
    public int? TryRealm(string? realmName)
    {
        if (realmName is null)
            return null;

        return Realm(realmName);
    }
    public int Realm(string realmName);

    public R GetRule<R>() where R : Rule;
    public RuleId GetRuleId<R>() where R : Rule;
    public R GetRule<R>(RuleId id) where R : Rule;
    public Rule GetRule(RuleId id);

    public C GetRuleClass<C>() where C : RuleClass;
    public RuleId GetRuleClassId<C>() where C : RuleClass;
    public C GetRuleClass<C>(RuleId id) where C : RuleClass;
    public RuleClass GetRuleClass(RuleId id);

    //===== TAST =====
    //NODES
    public TASTNode NodeAt(int nodeId);

    //TOKENS
    public int TokenType(string tokenName);
    public Token TokenAt(int tokenId);

    public ReadOnlySpan<char> GetTextSpan(int tokenId);
    public string GetText(int tokenId);
}
public partial class ParserProcess : Context
{
    public int Realm(string realmName)
    => Project.RealmId(realmName);

    public R GetRule<R>() where R : Rule
    => Project.GetRule<R>();
    public RuleId GetRuleId<R>() where R : Rule
    => Project.GetRuleId<R>();
    public R GetRule<R>(RuleId id) where R : Rule
    => Project.GetRule<R>(id);
    public Rule GetRule(RuleId id)
    => Project.GetRule(id);

    public C GetRuleClass<C>() where C : RuleClass
    => Project.GetRuleClass<C>();
    public RuleId GetRuleClassId<C>() where C : RuleClass
    => Project.GetRuleClassId<C>();
    public C GetRuleClass<C>(RuleId id) where C : RuleClass
    => Project.GetRuleClass<C>(id);
    public RuleClass GetRuleClass(RuleId id)
    => Project.GetRuleClass(id);

    //===== TAST =====
    //NODES
    public TASTNode NodeAt(int nodeId)
    => TAST.NodeAt(nodeId);

    //TOKENS
    public int TokenType(string tokenName)
    => Rules.Lexer.RuleExt.TokenTypeId(Project, tokenName);
    public Token TokenAt(int tokenId)
    => TAST.TokenAt(tokenId);

    public ReadOnlySpan<char> GetTextSpan(int tokenId)
    => TAST.GetTextSpan(tokenId);
    public string GetText(int tokenId)
    => TAST.GetText(tokenId);
}