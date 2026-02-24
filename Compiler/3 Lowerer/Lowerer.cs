using System.Diagnostics.CodeAnalysis;
using DrzSharp.Compiler.Lowerer;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler
{
    public partial class Compiler
    {
        public static LowererProcess NewLowerer() => LowererManager.NewProcess();

        public static void LowerProject(DzProject project)
        {
            var low = NewLowerer();
            low.LowerProject(project);
            low.EndProcess();
        }
    }
}

namespace DrzSharp.Compiler.Lowerer
{
    public delegate void Rule(Context ctx, Instruction instruction);

    public readonly struct RuleId(int id)
    {
        public readonly int Id = id;
        public bool Equals(RuleId other)
        => Id == other.Id;
    }

    internal static class LowererManager
    {
        //RULES
        public static readonly List<Rule> _rules = [];
        public static bool TryGetRule(RuleId ruleId, [NotNullWhen(true)] out Rule? rule)
        {
            rule = null;
            if (ruleId.Id < 0 || ruleId.Id >= _rules.Count)
                return false;
            
            rule = _rules[ruleId.Id];
            return true;
        }

        //PROCESSES
        public static LowererProcess NewProcess() => new();
        public static void EndProcess(this LowererProcess process) { }
    }
}