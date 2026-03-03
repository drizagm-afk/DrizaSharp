using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Parser;

/*=========================
       MATCH CONTEXT
=========================*/
public interface MatchContext : MatchView
{
    //VAR HASHES
    public int Hash { get; }
    public int NewHash();
    public void LoadHash(int hash);

    //COMMITS
    public int CommitCode { get; }
    public int Commit();
    public void Rollback(int commitCode);

    //STORE VAR
    public void StoreVar(int captureTag, TokenSpan var);

    //STORE RULEVAR
    public void StoreRuleVar(int captureTag, RuleInstance var);

    //EVAL RULE
    public RuleInstance? EvalRule<R>(TokenSpan span) where R : Rule;
    public RuleInstance? EvalRuleClass<C>(TokenSpan span) where C : RuleClass;

    //PATTERN MATCHING
    public bool TryTokenAtSpan(TokenSpan span, out Token token);
    public bool HasTokenAtSpan(TokenSpan span);
    public Token TokenAtSpan(TokenSpan span);
}

public partial class ParserProcess : MatchContext
{
    //===== INTERNAL STORAGE =====
    //VAR MANAGEMENT
    private readonly List<VarEntry> _matchVars = [];
    private readonly Dictionary<VarKey, int> _matchVarDict = [];
    private readonly Dictionary<RuleVarKey, RuleInstance> _matchRuleVarDict = [];

    private void ClearVarStorage()
    {
        _matchVars.Clear();
        _matchVarDict.Clear();
        _matchRuleVarDict.Clear();
    }

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

    //GENERAL
    private void InitMatch()
    {
        _hash = 0;
        _lastHash = 0;

        _commitCode = -1;
        _lastCommit = -1;
        Commit();
    }

    //===== EXPOSED FUNCTIONS =====
    //STORE VAR
    private void NewVar(VarKey key, TokenSpan var, int count = 0, int siblingId = -1)
    {
        var id = _matchVars.Count;
        _matchVars.Add(new(var, id, count + 1, siblingId, _commitCode));
        _matchVarDict[key] = id;
    }
    public void StoreVar(int tag, TokenSpan var)
    {
        var key = new VarKey(_hash, tag);
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

    //STORE RULEVAR
    public void StoreRuleVar(int tag, RuleInstance inst)
    {
        StoreVar(tag, inst.Span);
        _matchRuleVarDict[new(_hash, inst.Span.Start, inst.Span.Length)] = inst;
    }

    //RULE EVAL
    public RuleInstance? EvalRule<R>(TokenSpan span) where R : Rule
    => MatchRule(GetRule<R>(), span);
    public RuleInstance? EvalRuleClass<C>(TokenSpan span) where C : RuleClass
    => MatchRule(GetRuleClass<C>(), span);

    private bool EvalVarsFromNode(int entryId, TokenPattern pattern)
    {
        do
        {
            var entry = _matchVars[entryId];
            if (pattern.Matches(this, entry.Span) <= 0)
                return false;

            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return true;
    }
    private bool EvalVars(int varTag, TokenPattern pattern)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varTag), out var entryId))
            return true;

        CountVars(varTag);
        return EvalVarsFromNode(entryId, pattern);
    }

    //===== PATTERN MATCHING =====
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

internal readonly record struct VarKey
(int Hash, int Tag);
internal readonly struct VarEntry
(TokenSpan span, int id, int count, int siblingId, int commitCode)
{
    public readonly TokenSpan Span = span;
    public readonly int Id = id;
    public readonly int Count = count;
    public readonly int SiblingId = siblingId;
    public readonly int CommitCode = commitCode;
}

internal readonly record struct RuleVarKey
(int Hash, int Start, int Length);

/*=========================
        MATCH VIEW
=========================*/
public interface MatchView : Context
{
    //LOAD TOKEN VAR

    //LOAD VAR
    public TokenSpan LoadVar(int varTag);
    public bool TryLoadVar(int varTag, out TokenSpan var);
    public bool HasVar(int varTag);
    public int CountVars(int varTag);
    public TokenSpan[] LoadVars(int varTag);

    //LOAD TOKENVAR
    public Token LoadTokenVar(int varTag);
    public bool TryLoadTokenVar(int varTag, out Token var);
    public bool HasTokenVar(int varTag);
    public Token[] LoadTokenVars(int varTag);

    //LOAD RULEVAR
    public R LoadRuleVar<R>(int varTag) where R : RuleInstance;
    public bool TryLoadRuleVar<R>(int varTag, [NotNullWhen(true)] out R? ruleVar)
    where R : RuleInstance;
    public bool HasRuleVar(int varTag);
    public R[] LoadRuleVars<R>(int varTag) where R : RuleInstance;
}

public partial class ParserProcess : MatchView
{
    private bool TryGetEntry(int hash, int varTag, out VarEntry entry)
    {
        entry = default;
        VarKey key = new(hash, varTag);
        if (_matchVarDict.TryGetValue(key, out var varId))
        {
            var entryId = varId;
            do
            {
                entry = _matchVars[entryId];
                if (_validCommits[entry.CommitCode])
                {
                    if (entryId != varId)
                        _matchVarDict[key] = entryId;

                    return true;
                }
                entryId = entry.SiblingId;
            }
            while (entryId >= 0);
        }
        return false;
    }
    private bool TryGetVar(int hash, int varTag, out TokenSpan var)
    {
        var r = TryGetEntry(hash, varTag, out var entry);
        var = entry.Span;
        return r;
    }

    //LOADING VARS
    public TokenSpan LoadVar(int varTag)
    {
        if (!TryGetVar(_hash, varTag, out var var))
            throw new Exception($"VAR NOT FOUND: varTag={varTag}");

        return var;
    }

    public bool TryLoadVar(int varTag, out TokenSpan var)
    => TryGetVar(_hash, varTag, out var);

    public bool HasVar(int varTag)
    => TryGetVar(_hash, varTag, out _);

    public int CountVars(int varTag)
    {
        if (!TryGetEntry(_hash, varTag, out var entry)) return 0;
        return entry.Count;
    }

    private int FillVarsFromNode(int entryId, int i, Span<TokenSpan> ary)
    {
        do
        {
            var entry = _matchVars[entryId];

            ary[^(++i)] = entry.Span;
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return i;
    }
    public TokenSpan[] LoadVars(int varTag)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varTag), out var entryId)) return [];
        var ary = new TokenSpan[CountVars(varTag)];

        FillVarsFromNode(entryId, 0, ary.AsSpan());
        return ary;
    }

    //LOADING TOKENVARS
    private bool TryGetToken(TokenSpan span, out Token token)
    {
        token = default;
        if (span.Length <= 0)
            return false;

        token = TokenAtSpan(span);
        return true;
    }
    private bool HasToken(TokenSpan span)
    => span.Length > 0 && HasTokenAtSpan(span);

    public Token LoadTokenVar(int varTag)
    {
        var span = LoadVar(varTag);
        if (!TryGetToken(span, out var token))
            throw new Exception($"VAR NAME DOESN'T REFER TO A TOKEN VAR: varTag={varTag}");

        return token;
    }
    public bool TryLoadTokenVar(int varTag, out Token var)
    {
        var = default;
        if (!TryLoadVar(varTag, out var span))
            return false;

        return TryGetToken(span, out var);
    }
    public bool HasTokenVar(int varTag)
    {
        if (!TryLoadVar(varTag, out var span))
            return false;

        return HasToken(span);
    }
    public Token[] LoadTokenVars(int varTag)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varTag), out var entryId)) return [];
        var ary = new Token[CountVars(varTag)];

        int i = 0;
        do
        {
            var entry = _matchVars[entryId];

            if (!TryGetToken(entry.Span, out ary[^(++i)]))
                throw new Exception($"VAR NAME DOESN'T REFER TO A TOKEN VAR: varTag={varTag}");
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return ary;
    }

    //LOADING RULEVARS
    private bool TryGetRuleInst<R>
    (TokenSpan span, [NotNullWhen(true)] out R? inst, int varTagLog)
    where R : RuleInstance
    {
        inst = null;
        if (!_matchRuleVarDict.TryGetValue(new(_hash, span.Start, span.Length), out var val))
            return false;

        Debug.Assert(val is R, $"VAR IS NOT EXPECTED TYPE {typeof(R).Name}: varTag={varTagLog}");
        inst = (R)val;
        return true;
    }
    private bool HasRuleInst(TokenSpan span) => _matchRuleVarDict.ContainsKey(new(_hash, span.Start, span.Length));

    public R LoadRuleVar<R>(int varTag) where R : RuleInstance
    {
        var span = LoadVar(varTag);
        if (!TryGetRuleInst<R>(span, out var inst, varTag))
            throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: varTag={varTag}");

        return inst;
    }

    public bool TryLoadRuleVar<R>(int varTag, [NotNullWhen(true)] out R? inst)
    where R : RuleInstance
    {
        inst = null;
        if (!TryLoadVar(varTag, out var span))
            return false;

        return TryGetRuleInst(span, out inst, varTag);
    }

    public bool HasRuleVar(int varTag)
    {
        if (!TryLoadVar(varTag, out var span))
            return false;

        return HasRuleInst(span);
    }

    public R[] LoadRuleVars<R>(int varTag) where R : RuleInstance
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varTag), out var entryId)) return [];
        var ary = new R[CountVars(varTag)];

        int i = 0;
        do
        {
            var entry = _matchVars[entryId];

            if (!TryGetRuleInst(entry.Span, out ary[^(++i)]!, varTag))
                throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: varTag={varTag}");
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return ary;
    }
}