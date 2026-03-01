using System.Diagnostics;
using System.Text;

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
    public static class StringExt
    {
        public static string Repeat(this string value, int count)
        {
            if (count <= 0) return string.Empty;
            if (count == 1) return value;

            var sb = new StringBuilder(value.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(value);

            return sb.ToString();
        }
        public static string Repeat(this char value, int count)
        => new(value, count);
    }

    public readonly struct SourceSpan
    {
        public SourceSpan(string source) : this(source, 0) { }
        public SourceSpan(string source, int start) : this(source, start, source.Length - start) { }
        public SourceSpan(string source, int start, int length)
        {
            Source = source;
            Start = start;
            Length = length;

            Debug.Assert(Start >= 0 && Length >= 0 && Start + Length <= Source.Length);
        }

        public readonly string Source;
        public readonly int Start;
        public readonly int Length;

        public char this[int i] { get => Source[Start + i]; }
        public ReadOnlySpan<char> AsSpan() => AsSpan(0, Length);
        public ReadOnlySpan<char> AsSpan(int start) => AsSpan(start, Length - start);
        public ReadOnlySpan<char> AsSpan(int start, int length)
        {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
            return Source.AsSpan(Start + start, length);
        }
        public ReadOnlySpan<char> AsSpan(Slice range)
        => AsSpan(range.Start, range.Length);

        public string Slice() => Slice(0, Length);
        public string Slice(int start) => Slice(start, Length - start);
        public string Slice(int start, int length)
        {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= Length);
            return Source.Substring(Start + start, length);
        }
        public string Slice(Slice range)
        => Slice(range.Start, range.Length);
    }
}