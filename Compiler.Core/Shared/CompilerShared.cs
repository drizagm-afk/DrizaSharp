using System.Diagnostics;

namespace DrzSharp.Compiler.Core;

public readonly struct StringSpan
{
    public StringSpan(string str) : this(str, 0) { }
    public StringSpan(string str, int start) : this(str, start, str.Length - start) { }
    public StringSpan(string str, int start, int length)
    {
        String = str;
        Start = start;
        Length = length;

        Debug.Assert(Start >= 0 && Length >= 0 && Start + Length <= String.Length);
    }

    public readonly string String;
    public readonly int Start;
    public readonly int Length;

    public char this[int i] { get => String[Start + i]; }
    public ReadOnlySpan<char> AsSpan() => String.AsSpan(0, Length);
    public ReadOnlySpan<char> AsSpan(int start) => String.AsSpan(start, Length - start);
    public ReadOnlySpan<char> AsSpan(int start, int length)
    {
        Debug.Assert(Start <= start && start + length <= Start + Length);
        return String.AsSpan(Start + start, length);
    }
}

public readonly struct Span(int start, int length)
{
    public readonly int Start = start;
    public readonly int Length = length;
}

public readonly struct RealmId(int phaseCode, int realmCode)
{
    public readonly int PhaseCode = phaseCode;
    public readonly int RealmCode = realmCode;

    public bool Equals(RealmId other)
    => PhaseCode == other.PhaseCode && RealmCode == other.RealmCode;
}