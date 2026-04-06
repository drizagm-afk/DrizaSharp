using DrzSharp.Compiler.Rules;

namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    //>>>> RULESET <<<<
    private readonly Ruleset _ruleset = new();
    internal Ruleset GetRuleset() => _ruleset;
}