using System.Diagnostics;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler;

public static partial class Compiler
{
    public static CompilerToolkit Toolkit = new();

    public static void Bind()
    {
        Toolkit.Stopwatch = Stopwatch.StartNew();

        Default.Bindings.Bind();
        Toolkit.ProcessTime.Add(("BINDINGS", Toolkit.Stopwatch.Elapsed.TotalMilliseconds));
    }

    public static void Compile(string root, string target)
    {
        //1. PROJECT EVALUATION
        var proj = Toolkit.Project = EvalProject(root, target);
        Toolkit.ProcessTime.Add(("EVALUATION", Toolkit.Stopwatch.Elapsed.TotalMilliseconds));

        //2. TOKENIZATION (LEXER)
        LexProject(proj);
        Toolkit.ProcessTime.Add(("LEXING", Toolkit.Stopwatch.Elapsed.TotalMilliseconds));

        //3. PARSING (PARSER)
        ParseProject(proj);
        Toolkit.ProcessTime.Add(("PARSING", Toolkit.Stopwatch.Elapsed.TotalMilliseconds));

        //4. LOWERING (LOWERER)
        //Toolkit.ProcessTime.Add(("LOWERING", Toolkit.Stopwatch.Elapsed.TotalMilliseconds));

        Toolkit.Stopwatch.Stop();
    }

    public static void Debug()
    {
        double total = 0;
        foreach ((var name, var time) in Toolkit.ProcessTime)
        {
            Console.WriteLine(name + " TIME: " + (time - total));
            total = time;
        }

        Console.WriteLine("COMPILING TIME: " + total);
    }
}

public class CompilerToolkit
{
    internal Stopwatch Stopwatch = null!;
    internal readonly List<(string, double)> ProcessTime = [];

    public DzProject Project = null!;
    public DzFile File { get; internal set; } = null!;
}