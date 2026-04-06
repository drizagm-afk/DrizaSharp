using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Patterns;

public static class Shortcuts
{
    public static Pattern Opt(this Pattern patt, Action<Pattern> optionalPatt)
    => patt.Optional(optionalPatt);
    public static Pattern OptNl(this Pattern patt, int? captureTag = null)
    => patt.Optional(t => t.nl(captureTag));
}