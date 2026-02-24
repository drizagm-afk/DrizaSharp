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
        public static void LowerFile(DzFile file)
        {
            var low = NewLowerer();
            low.LowerFile(file);
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

        //PROCESSES
        public static LowererProcess NewProcess() => new();
        public static void EndProcess(this LowererProcess process) { }
    }
}