using DrzSharp.Compiler.Evaluator;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler
{
    public static partial class Compiler
    {
        public static EvalProcess NewEvaluator() => EvalManager.NewProcess();

        public static DzProject EvalProject(string root, string target)
        {
            var eval = NewEvaluator();
            var proj = eval.EvalProject(root, Path.Combine(root, target));
            eval.EndProcess();
            return proj;
        }
    }
}

namespace DrzSharp.Compiler.Evaluator
{
    internal static class EvalManager
    {
        //PROCESSES
        public static EvalProcess NewProcess() => new();
        public static void EndProcess(this EvalProcess process) { }
    }
}