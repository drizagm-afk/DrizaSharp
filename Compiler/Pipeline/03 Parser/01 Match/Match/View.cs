using System.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public interface MatchView : Context
{
    //LOAD VARS
    public TokenSpan LoadVar(int captureTag);
    public bool TryLoadVar(int captureTag, out TokenSpan var);
    public bool HasVar(int captureTag);
    public int CountVars(int captureTag);
    public TokenSpan[] LoadVars(int captureTag);

    //LOAD TOKENVARS
    public Token LoadTokenVar(int captureTag);
    public bool TryLoadTokenVar(int captureTag, out Token var);
    public bool HasTokenVar(int captureTag);
    public Token[] LoadTokenVars(int captureTag);

    //LOAD RULEVARS
    public R LoadRuleVar<R>(int captureTag) where R : RuleInstance;
    public bool TryLoadRuleVar<R>(int captureTag, out R ruleVar) where R : RuleInstance;
    public bool HasRuleVar(int captureTag);
    public R[] LoadRuleVars<R>(int captureTag) where R : RuleInstance;
}

public partial class ParserProcess : MatchView
{
    //===== VARS =====
    private bool TryGetEntry(int hash, int captureTag, out VarEntry entry)
    {
        entry = default;
        VarKey key = new(hash, captureTag);
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
    private bool TryGetVar(int hash, int captureTag, out TokenSpan var)
    {
        var r = TryGetEntry(hash, captureTag, out var entry);
        var = entry.Span;
        return r;
    }

    //LOAD VARS
    public TokenSpan LoadVar(int captureTag)
    {
        if (!TryGetVar(_hash, captureTag, out var var))
            throw new Exception($"VAR NOT FOUND: captureTag={captureTag}");

        return var;
    }
    public bool TryLoadVar(int captureTag, out TokenSpan var)
    => TryGetVar(_hash, captureTag, out var);
    public bool HasVar(int captureTag)
    => TryGetVar(_hash, captureTag, out _);
    public int CountVars(int captureTag)
    {
        if (!TryGetEntry(_hash, captureTag, out var entry)) return 0;
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
    public TokenSpan[] LoadVars(int captureTag)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, captureTag), out var entryId)) return [];
        var ary = new TokenSpan[CountVars(captureTag)];

        FillVarsFromNode(entryId, 0, ary.AsSpan());
        return ary;
    }

    //===== TOKENVARS =====
    //LOAD TOKENVARS
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
    public Token LoadTokenVar(int captureTag)
    {
        var span = LoadVar(captureTag);
        if (!TryGetToken(span, out var token))
            throw new Exception($"VAR NAME DOESN'T REFER TO A TOKEN VAR: captureTag={captureTag}");

        return token;
    }
    public bool TryLoadTokenVar(int captureTag, out Token var)
    {
        var = default;
        if (!TryLoadVar(captureTag, out var span))
            return false;

        return TryGetToken(span, out var);
    }
    public bool HasTokenVar(int captureTag)
    {
        if (!TryLoadVar(captureTag, out var span))
            return false;

        return HasToken(span);
    }
    public Token[] LoadTokenVars(int captureTag)
    {
        if (!_matchVarDict.TryGetValue(new(_hash, captureTag), out var entryId)) return [];
        var ary = new Token[CountVars(captureTag)];

        int i = 0;
        do
        {
            var entry = _matchVars[entryId];

            if (!TryGetToken(entry.Span, out ary[^(++i)]))
                throw new Exception($"VAR NAME DOESN'T REFER TO A TOKEN VAR: captureTag={captureTag}");
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return ary;
    }

    //===== RULEVARS =====
    private bool TryGetRuleInst<R>
    (TokenSpan span, out R inst, int captureTagLog)
    where R : RuleInstance
    {
        inst = null!;
        if (!_matchRuleVarDict.TryGetValue(new(_hash, span.Start, span.Length), out var val))
            return false;

        Debug.Assert(val is R, $"VAR IS NOT EXPECTED TYPE {typeof(R).Name}: captureTag={captureTagLog}");
        inst = (R)val;
        return true;
    }
    private bool HasRuleInst(TokenSpan span) => _matchRuleVarDict.ContainsKey(new(_hash, span.Start, span.Length));

    //LOAD RULEVARS
    public R LoadRuleVar<R>(int captureTag) where R : RuleInstance
    {
        var span = LoadVar(captureTag);
        if (!TryGetRuleInst<R>(span, out var inst, captureTag))
            throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: captureTag={captureTag}");

        return inst;
    }
    public bool TryLoadRuleVar<R>(int captureTag, out R inst) where R : RuleInstance
    {
        inst = null!;
        if (!TryLoadVar(captureTag, out var span))
            return false;

        return TryGetRuleInst(span, out inst, captureTag);
    }
    public bool HasRuleVar(int captureTag)
    {
        if (!TryLoadVar(captureTag, out var span))
            return false;

        return HasRuleInst(span);
    }
    public R[] LoadRuleVars<R>(int captureTag) where R : RuleInstance
    {
        if (!_matchVarDict.TryGetValue(new(_hash, captureTag), out var entryId)) return [];
        var ary = new R[CountVars(captureTag)];

        int i = 0;
        do
        {
            var entry = _matchVars[entryId];

            if (!TryGetRuleInst(entry.Span, out ary[^(++i)]!, captureTag))
                throw new Exception($"VAR NAME DOESN'T REFER TO A RULE VAR: captureTag={captureTag}");
            entryId = entry.SiblingId;
        }
        while (entryId >= 0);
        return ary;
    }
}