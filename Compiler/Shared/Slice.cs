namespace DrzSharp.Compiler;

//>>>> SLICE <<<<
public readonly struct Slice(int start, int length)
{
    public readonly int Start = start;
    public readonly int Length = length;
    public bool IsValid => Length > 0;
}