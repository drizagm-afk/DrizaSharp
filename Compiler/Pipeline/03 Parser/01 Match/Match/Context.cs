using System.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface MatchContext : MatchView
{
    //HASHES
    public int Hash { get; }
    public int NewHash();
    public void LoadHash(int hash);

    //COMMITS
    public int CommitCode { get; }
    public int Commit();
    public void Rollback(int commitCode);

    //===== VARS =====
    public void StoreVar(int captureTag, TokenSpan var);
    public void StoreRuleVar(int captureTag, RuleInstance var);

    //===== MATCH =====
    //RECURSIVE MATCHING
    public RuleInstance? MatchRule<R>(TokenSpan span) where R : Rule;
    public RuleInstance? MatchRuleClass<C>(TokenSpan span) where C : RuleClass;
    public RuleInstance? MatchRealm(int realm, TokenSpan span);

    //PATTERN MATCHING
    public bool TryTokenAtSpan(TokenSpan span, out Token token);
    public bool HasTokenAtSpan(TokenSpan span);
    public Token TokenAtSpan(TokenSpan span);
}

public partial class ParserProcess : MatchContext
{
    //HASHES
    private int _hash;
    private int _lastHash;

    public int Hash => _hash;
    public int NewHash()
    => _hash = ++_lastHash;
    public void LoadHash(int hash)
    => _hash = hash;

    //COMMITS
    private bool[] _validCommits = new bool[16];
    private int _commitCode;
    private int _lastCommit;

    public int CommitCode => _commitCode;
    public int Commit()
    {
        _commitCode = ++_lastCommit;

        var len = _validCommits.Length;
        if (len <= _commitCode)
            Array.Resize(ref _validCommits, len * 2);

        _validCommits[_commitCode] = true;
        return _commitCode;
    }
    public void Rollback(int commitCode)
    {
        Debug.Assert(commitCode <= _commitCode, $"TRIED TO LOAD UNVALID COMMIT: commitCode={commitCode}");
        if (commitCode < _commitCode)
        {
            int start = commitCode + 1;
            int count = _commitCode - commitCode;
            Array.Fill(_validCommits, false, start, count);
        }

        _commitCode = commitCode;
    }

    //===== VARS =====
    private readonly List<VarEntry> _matchVars = [];
    private readonly Dictionary<VarKey, int> _matchVarDict = [];
    private readonly Dictionary<RuleVarKey, RuleInstance> _matchRuleVarDict = [];

    internal readonly struct VarEntry
    (TokenSpan span, int id, int count, int siblingId, int commitCode)
    {
        public readonly TokenSpan Span = span;
        public readonly int Id = id;
        public readonly int Count = count;
        public readonly int SiblingId = siblingId;
        public readonly int CommitCode = commitCode;
    }
    internal readonly record struct VarKey
    (int Hash, int CaptureTag);
    internal readonly record struct RuleVarKey
    (int Hash, int Start, int Length);

    //STORE VAR
    private void NewVar(VarKey key, TokenSpan var, int count = 0, int siblingId = -1)
    {
        var id = _matchVars.Count;
        _matchVars.Add(new(var, id, count + 1, siblingId, _commitCode));
        _matchVarDict[key] = id;
    }
    public void StoreVar(int captureTag, TokenSpan var)
    {
        var key = new VarKey(_hash, captureTag);
        if (_matchVarDict.TryGetValue(key, out var entryId))
        {
            do
            {
                var entry = _matchVars[entryId];
                if (_validCommits[entry.CommitCode])
                {
                    NewVar(key, var, entry.Count, entryId);
                    return;
                }
                entryId = entry.SiblingId;
            }
            while (entryId >= 0);
        }
        NewVar(key, var);
    }
    public void StoreRuleVar(int captureTag, RuleInstance inst)
    {
        StoreVar(captureTag, inst.Span);
        _matchRuleVarDict[new(_hash, inst.Span.Start, inst.Span.Length)] = inst;
    }

    //===== MATCH =====
    //RECURSIVE MATCHING
    public RuleInstance? MatchRule<R>(TokenSpan span) where R : Rule
    => MatchRule(GetRule<R>(), span);
    public RuleInstance? MatchRuleClass<C>(TokenSpan span) where C : RuleClass
    => MatchRuleClass(GetRuleClassId<C>(), span);
    public RuleInstance? MatchRealm(int realm, TokenSpan span)
    {
        foreach (var rule in Project.RulesPerRealm(Module, realm))
        {
            if (rule.IsAbstract) continue;

            //MATCH
            var inst = MatchRule(rule, span);

            if (inst is not null)
                return inst;
        }
        return null;
    }

    //PATTERN MATCHING
    public bool TryTokenAtSpan(TokenSpan span, out Token token)
    {
        if (!span.IsValid)
        {
            token = default;
            return false;
        }

        return TAST.TryTokenAtNode(span, out token);
    }
    public bool HasTokenAtSpan(TokenSpan span)
    => span.IsValid && TAST.HasTokenAtNode(span);
    public Token TokenAtSpan(TokenSpan span)
    {
        if (!span.IsValid)
            throw new Exception($"INVALID TOKEN SPAN: offset={span.Offset}, start={span.Start}, length={span.Length}");

        return TAST.TokenAtNode(span);
    }
}