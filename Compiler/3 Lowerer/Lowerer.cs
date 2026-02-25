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

    internal static class LowererManager
    {
        //PROCESSES
        public static LowererProcess NewProcess() => new();
        public static void EndProcess(this LowererProcess process) { }

        //===== RULES =====
        public static readonly List<Rule> _rules = [];
        public static bool TryGetRule(int id, [NotNullWhen(true)] out Rule? rule)
        {
            rule = null;
            if (id < 0 || id >= _rules.Count)
                return false;

            rule = _rules[id];
            return true;
        }
    }
}