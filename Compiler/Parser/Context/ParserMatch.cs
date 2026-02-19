using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Parser;

/*=========================
       MATCH CONTEXT
=========================*/
public interface ParserMatchContext : ParserMatchView
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
    public void StoreVar(string captureTag, TokenSpan var);

    //STORE RULEVAR
    public void StoreRuleVar(string captureTag, ParserRuleInstance var);

    //EVAL RULE
    public ParserRuleInstance? EvalRule<R>(TokenSpan span) where R : ParserRule;
    public ParserRuleInstance? EvalRuleClass<C>(TokenSpan span) where C : ParserRuleClass;
}

public partial class ParserProcess : ParserMatchContext
{
    //===== INTERNAL STORAGE =====
    //VAR MANAGEMENT
    private readonly List<VarEntry> _matchVars = [];
    private readonly Dictionary<VarKey, int> _matchVarDict = [];
    private readonly Dictionary<RuleVarKey, ParserRuleInstance> _matchRuleVarDict = [];

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
    private bool[] _validCommits = [];
    private int _commitCode;
    private int _lastCommit;

    public int CommitCode => _commitCode;
    public int Commit()
    {
        _commitCode = ++_lastCommit;

        if (_validCommits.Length <= 0)
            _validCommits = new bool[16];
        else if (_validCommits.Length <= _commitCode)
            Array.Resize(ref _validCommits, _validCommits.Length * 2);

        _validCommits[_commitCode] = true;
        return _commitCode;
    }
    public void Rollback(int commitCode)
    {
        Debug.Assert(commitCode <= _commitCode, $"TRIED TO LOAD UNVALID COMMIT: commitCode={commitCode}");
        if (commitCode < _commitCode)
        {
            var start = commitCode + 1;
            Array.Fill(_validCommits, false, start, _commitCode - start);
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
    public void StoreVar(string tag, TokenSpan var)
    {
        var key = new VarKey(_hash, tag);
        if (_matchVarDict.TryGetValue(key, out var nodeId))
        {
            do
            {
                var node = _matchVars[nodeId];
                if (_validCommits[node.CommitCode])
                {
                    NewVar(key, var, node.Count, node.SiblingId);
                    return;
                }
                nodeId = node.SiblingId;
            }
            while (nodeId >= 0);
        }
        NewVar(key, var);
    }

    //STORE RULEVAR
    public void StoreRuleVar(string tag, ParserRuleInstance inst)
    {
        StoreVar(tag, inst.Span);
        _matchRuleVarDict[new(_hash, inst.Span.Start, inst.Span.Length)] = inst;
    }

    //RULE EVAL
    public ParserRuleInstance? EvalRule<R>(TokenSpan span) where R : ParserRule
    => EvalRuleMatch(GetRule<R>(), span);
    public ParserRuleInstance? EvalRuleClass<C>(TokenSpan span) where C : ParserRuleClass
    => EvalRuleMatch(GetRuleClass<C>(), span);

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
    private bool EvalVars(string varName, TokenPattern pattern)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varName), out var entryId))
            return true;

        CountVars(varName);
        return EvalVarsFromNode(entryId, pattern);
    }
}

internal readonly record struct VarKey
(int Hash, string Name);
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
public interface ParserMatchView : ParserContext
{
    //LOAD VAR
    public TokenSpan LoadVar(string varName);
    public bool TryLoadVar(string varName, out TokenSpan var);
    public bool HasVar(string varName);
    public int CountVars(string varName);
    public TokenSpan[] LoadVars(string varName);

    //LOAD RULEVAR
    public R LoadRuleVar<R>(string varName) where R : ParserRuleInstance;
    public bool TryLoadRuleVar<R>(string varName, [NotNullWhen(true)] out R? ruleVar)
    where R : ParserRuleInstance;
    public bool HasRuleVar(string varName);
    public R[] LoadRuleVars<R>(string varName) where R : ParserRuleInstance;
}

public partial class ParserProcess : ParserMatchView
{
    private bool TryGetEntry(int hash, string varName, out VarEntry entry)
    {
        entry = default;
        VarKey key = new(hash, varName);
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
    private bool TryGetVar(int hash, string varName, out TokenSpan var)
    {
        var r = TryGetEntry(hash, varName, out var entry);
        var = entry.Span;
        return r;
    }

    //LOADING VARS
    public TokenSpan LoadVar(string varName)
    {
        if (!TryGetVar(_hash, varName, out var var))
            throw new Exception($"VAR NOT FOUND: varName={varName}");
        
        return var;
    }

    public bool TryLoadVar(string varName, out TokenSpan var)
    => TryGetVar(_hash, varName, out var);

    public bool HasVar(string varName) 
    => TryGetVar(_hash, varName, out _);

    public int CountVars(string varName)
    {
        if (!TryGetEntry(_hash, varName, out var entry)) return 0;
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
    public TokenSpan[] LoadVars(string varName)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varName), out var entryId)) return [];
        var ary = new TokenSpan[CountVars(varName)];

        FillVarsFromNode(entryId, 0, ary.AsSpan());
        return ary;
    }

    //LOADING RULEVARS
    private bool TryGetRuleInst<R>
    (TokenSpan span, [NotNullWhen(true)] out R? inst, string varNameLog)
    where R : ParserRuleInstance
    {
        inst = null;
        if (!_matchRuleVarDict.TryGetValue(new(_hash, span.Start, span.Length), out var val))
            return false;

        Debug.Assert(val is R, $"VAR IS NOT EXPECTED TYPE {typeof(R).Name}: varName={varNameLog}");
        inst = (R)val;
        return true;
    }
    private bool HasRuleInst(TokenSpan span) => _matchRuleVarDict.ContainsKey(new(_hash, span.Start, span.Length));

    public R LoadRuleVar<R>(string varName) where R : ParserRuleInstance
    {
        var span = LoadVar(varName);
        if (!TryGetRuleInst<R>(span, out var inst, varName))
            throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: varName={varName}");

        return inst;
    }

    public bool TryLoadRuleVar<R>(string varName, [NotNullWhen(true)] out R? inst)
    where R : ParserRuleInstance
    {
        inst = null;
        if (!TryLoadVar(varName, out var span))
            return false;

        return TryGetRuleInst(span, out inst, varName);
    }

    public bool HasRuleVar(string varName)
    {
        if (!TryLoadVar(varName, out var span))
            return false;

        return HasRuleInst(span);
    }

    public R[] LoadRuleVars<R>(string varName) where R : ParserRuleInstance
    {
        if (!_matchVarDict.TryGetValue(new(_hash, varName), out var entryId)) return [];
        var ary = new R[CountVars(varName)];

        int i = 0;
        do
        {
            var entry = _matchVars[entryId];

            if (!TryGetRuleInst(entry.Span, out ary[^(++i)]!, varName))
                throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: varName={varName}");
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return ary;
    }
}