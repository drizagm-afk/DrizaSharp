using DrzSharp.Compiler.Evaluator;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler
{
    public static partial class Compiler
    {
        public static DzProject EvalProject(string root, string target)
        {
            return EvalProcess.EvalProject(root, Path.Combine(root, target));
        }
    }
}

namespace DrzSharp.Compiler.Evaluator
{
    public static class EvalProcess
    {
        const byte SINGLE_FILE_PROJ = 0;
        const byte MULTI_FILE_PROJ = 0;

        public static byte ProjectType(string path)
        {
            return Path.GetExtension(path) switch
            {
                ".dz" => 0,
                ".dzproj" => 1,
                _ => throw new Exception("INVALID TARGET FILE")
            };
        }

        public static DzProject EvalProject(string root, string path)
        {
            var proj = new DzProject(path, ProjectType(path));

            //LOAD DEFAULT DEPENDENCIES
            VirtualEval.LoadAssembly(proj.VWorld, @"C:\Driza\DrizaSharp\packages\System.Private.CoreLib.dll");

            //LOAD FILES
            if (proj.Type == SINGLE_FILE_PROJ)
            {
                EvalFile(proj, path);
            }

            return proj;
        }

        public static void EvalFile(DzProject proj, string path)
        {
            //TAST INITIATION
            var content = new StringSpan(File.ReadAllText(path));
            var dzFile = new DzFile(path, content);

            proj.Files.Add(dzFile);
        }
    }
}