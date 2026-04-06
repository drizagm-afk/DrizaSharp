namespace DrzSharp.Compiler.Rules;

public interface BindingContext
{
    public LexerBinding Lexer { get; }
    public ParserBinding Parser { get; }
}

internal partial class Ruleset
{
    internal readonly RulesetBinding Binding;
    internal Ruleset()
    {
        Binding = new(this);
    }
}
internal partial class RulesetBinding : BindingContext
{
    public LexerBinding Lexer => this;
    public ParserBinding Parser => this;

    private readonly Ruleset _ruleset;
    internal RulesetBinding(Ruleset ruleset)
    => _ruleset = ruleset;

    private int _assemblyId;
    private int _nspaceId;
    internal void Initialize(int assemblyId, int nspaceId)
    {
        _assemblyId = assemblyId;
        _nspaceId = nspaceId;
    }

    private LocalRuleset _localRuleset
    {
        get
        {
            if (!_ruleset._localRulesets.TryGetValue(_assemblyId, out var assemblyRuleset))
                _ruleset._localRulesets[_assemblyId] = assemblyRuleset = [];
            
            if (!assemblyRuleset.TryGetValue(_nspaceId, out var localRuleset))
                assemblyRuleset[_nspaceId] = localRuleset = new();

            return localRuleset;
        }
    }
    private RuleId NewClassRuleId(int localId)
    => new(_assemblyId, -1, localId);
    private RuleId NewRuleId(int localId)
    => new(_assemblyId, _nspaceId, localId);
}