using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface Context
{
    //===== PROJECT =====
    public string ProjectPath();
    public DzProjectType ProjectType();

    public int FileId();
    public string FilePath();

    public bool IsFirstFile();
    public bool IsLastFile();

    //===== RULESET =====
    public int Realm(string realmName);
    public int? TryRealm(string? realmName)
    {
        if (realmName is null)
            return null;

        return Realm(realmName);
    }

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
    public ref readonly TASTNode NodeAt(int nodeId);
    public T ResolveNode<T>(int nodeId) where T : RuleInstance;
    public bool TryResolveNode<T>(int nodeId, out T inst) where T : RuleInstance;

    public ref readonly TASTNode NodeAt(FileNodeId nodeId);
    public T ResolveNode<T>(FileNodeId nodeId) where T : RuleInstance;
    public bool TryResolveNode<T>(FileNodeId nodeId, out T inst) where T : RuleInstance;

    //TOKENS
    public int TokenType(string tokenName);

    public Token TokenAt(int tokenId);
    public ReadOnlySpan<char> GetTextSpan(int tokenId);
    public string GetText(int tokenId);

    public Token TokenAt(FileNodeId tokenId);
    public ReadOnlySpan<char> GetTextSpan(FileNodeId tokenId);
    public string GetText(FileNodeId tokenId);
}
public partial class ParserProcess : Context
{
    //===== PROJECT =====
    public string ProjectPath()
    => Project.Path;
    public DzProjectType ProjectType()
    => Project.Type;

    public int FileId()
    => File.Id;
    public string FilePath()
    => File.Path;

    public bool IsFirstFile()
    => File.Id == 0;
    public bool IsLastFile()
    => File.Id == Project.Files.Length - 1;

    //===== RULESET =====
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
    private TAST TASTAt(FileNodeId nodeId)
    => Project.Files[nodeId.FileId].TAST;

    //NODES
    public ref readonly TASTNode NodeAt(int nodeId)
    => ref TAST.NodeAt(nodeId);
    public T ResolveNode<T>(int nodeId) where T : RuleInstance
    => (T)TAST.GetApplyRule(nodeId);
    public bool TryResolveNode<T>(int nodeId, out T inst) where T : RuleInstance
    {
        var res = TAST.TryGetApplyRule(nodeId, out var binst);
        inst = (T)binst!;

        return res;
    }

    public ref readonly TASTNode NodeAt(FileNodeId nodeId)
    => ref TASTAt(nodeId).NodeAt(nodeId.NodeId);
    public T ResolveNode<T>(FileNodeId nodeId) where T : RuleInstance
    => (T)TASTAt(nodeId).GetApplyRule(nodeId.NodeId);
    public bool TryResolveNode<T>(FileNodeId nodeId, out T inst) where T : RuleInstance
    {
        var res = TASTAt(nodeId).TryGetApplyRule(nodeId.NodeId, out var binst);
        inst = (T)binst!;

        return res;
    }

    //TOKENS
    public int TokenType(string tokenName)
    => Rules.Lexer.RuleExt.TokenTypeId(Project, tokenName);

    public Token TokenAt(int tokenId)
    => TAST.TokenAt(tokenId);
    public ReadOnlySpan<char> GetTextSpan(int tokenId)
    => TAST.GetTextSpan(tokenId);
    public string GetText(int tokenId)
    => TAST.GetText(tokenId);

    public Token TokenAt(FileNodeId tokenId)
    => TASTAt(tokenId).TokenAt(tokenId.NodeId);
    public ReadOnlySpan<char> GetTextSpan(FileNodeId tokenId)
    => TASTAt(tokenId).GetTextSpan(tokenId.NodeId);
    public string GetText(FileNodeId tokenId)
    => TASTAt(tokenId).GetText(tokenId.NodeId);
}