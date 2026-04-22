using System.Collections;
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