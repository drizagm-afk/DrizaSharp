using System.Diagnostics;

namespace DrzSharp.Compiler;

public readonly struct SourceSlice(int start, int length)
{
    public readonly int Start = start;
    public readonly int Length = length;
    public bool IsValid => Length > 0;
}
public readonly struct SourceText
{
    public SourceText(string text) : this(text, 0) { }
    public SourceText(string text, int start) : this(text, start, text.Length - start) { }
    public SourceText(string text, int start, int length)
    {
        Text = text;
        Start = start;
        Length = length;

        Debug.Assert(Start >= 0 && Length >= 0 && Start + Length <= Text.Length);
    }

    public readonly string Text;
    public readonly int Start;
    public readonly int Length;

    public char this[int i] { get => Text[Start + i]; }
    public ReadOnlySpan<char> AsSpan() => AsSpan(0, Length);
    public ReadOnlySpan<char> AsSpan(int start) => AsSpan(start, Length - start);
    public ReadOnlySpan<char> AsSpan(int start, int length)
    {
        Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
        return Text.AsSpan(Start + start, length);
    }
    public ReadOnlySpan<char> AsSpan(SourceSlice range)
    => AsSpan(range.Start, range.Length);

    public string Slice() => Slice(0, Length);
    public string Slice(int start) => Slice(start, Length - start);
    public string Slice(int start, int length)
    {
        Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
        return Text.Substring(Start + start, length);
    }
    public string Slice(SourceSlice range)
    => Slice(range.Start, range.Length);
}