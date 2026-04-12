using System.Text;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler;

//>>>> GLOBAL METHOD EXTENSIONS <<<<
public static class Ext
{
    //STRING
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

    public static string If(this string value, bool condition)
    => condition ? value : string.Empty;

    //LIST
    public static T RemoveLast<T>(this List<T> list)
    {
        var last = list[^1];
        list.RemoveAt(list.Count - 1);

        return last;
    }
}

//>>>> EMPTY COLLECTIONS <<<<
public static class Empty
{
    //>>>> VIRTUAL <<<<
    public static readonly IReadOnlyDictionary<string, int> IdByName = new Dictionary<string, int>(0);
    public static readonly IReadOnlyDictionary<string, List<int>> IdListByName = new Dictionary<string, List<int>>(0);

    public static readonly IReadOnlyDictionary<GenName, int> IdByGenName = new Dictionary<GenName, int>(0);
    public static readonly IReadOnlyDictionary<GenName, List<int>> IdListByGenName = new Dictionary<GenName, List<int>>(0);
    
    public static readonly IReadOnlyList<int> IdList = new List<int>(0);

    //>>>> COMMON <<<<
    public static readonly IReadOnlyList<string> StringList = new List<string>(0);
}

//>>>> SLICE <<<<
public readonly struct Slice(int start, int length)
{
    public readonly int Start = start;
    public readonly int Length = length;
    public bool IsValid => Length > 0;
}