using System.Diagnostics;

namespace DrzSharp.Compiler
{
    public readonly struct Slice(int start, int length)
    {
        public readonly int Start = start;
        public readonly int Length = length;
        public readonly bool IsValid => Length > 0;
    }
}

namespace DrzSharp.Compiler.Text
{
    public readonly struct SourceSpan
    {
        public SourceSpan(string str) : this(str, 0) { }
        public SourceSpan(string str, int start) : this(str, start, str.Length - start) { }
        public SourceSpan(string str, int start, int length)
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
        public ReadOnlySpan<char> AsSpan() => AsSpan(0, Length);
        public ReadOnlySpan<char> AsSpan(int start) => AsSpan(start, Length - start);
        public ReadOnlySpan<char> AsSpan(int start, int length)
        {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
            return String.AsSpan(Start + start, length);
        }
        public ReadOnlySpan<char> AsSpan(Slice range)
        => AsSpan(range.Start, range.Length);

        public string Slice() => Slice(0, Length);
        public string Slice(int start) => Slice(start, Length - start);
        public string Slice(int start, int length)
        {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
            return String.Substring(Start + start, length);
        }
        public string Slice(Slice range)
        => Slice(range.Start, range.Length);
    }
}