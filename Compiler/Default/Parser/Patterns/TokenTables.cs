using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Patterns;

public static class TokenTables
{
    public static Pattern KwTable(this Pattern patt, int? captureTag, params string[] vals)
    {
        patt.AddPattern((_, ctx, span) =>
        {
            //MATCH
            var token = ctx.TokenAtSpan(span);
            if (token.Type != TokenType.Keyword)
                return 0;

            for (int i = 0; i < vals.Length; i++)
            {
                var val = vals[i];
                var txt = ctx.GetTextSpan(token.Id);

                if (txt.Length != val.Length)
                    continue;
                if (!txt.StartsWith(val))
                    continue;

                if (captureTag is int tag)
                    ctx.StoreVar(tag, span.With(length: i));
                return 1;
            }
            return 0;
        });

        return patt;
    }

    public static byte LoadTableVar(this MatchView view, int captureTag)
    => (byte)view.LoadVar(captureTag).Length;
    public static bool TryLoadTableVar(this MatchView view, int captureTag, out byte val)
    {
        if (view.TryLoadVar(captureTag, out var span))
        {
            val = (byte)span.Length;
            return true;
        }
        val = 0;
        return false;
    }

    public static T LoadTableVar<T>(this MatchView view, int captureTag) where T : Enum
    {
        var byteVal = view.LoadTableVar(captureTag);
        return (T)Enum.ToObject(typeof(T), byteVal);
    }
    public static bool TryLoadTableVar<T>(this MatchView view, int captureTag, out T val) where T : Enum
    {
        var trial = view.TryLoadTableVar(captureTag, out var byteVal);

        val = (T)Enum.ToObject(typeof(T), byteVal);
        return trial;
    }
}