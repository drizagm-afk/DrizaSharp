using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler;

public static partial class Compiler
{
    public static void Bind()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Default.Bindings.Bind();
        sw.Stop();

        Console.WriteLine($"BINDING TIME: {sw.Elapsed.TotalMilliseconds:F4}");
    }
    public static void Compile(string root, string target)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<(string, double)> procTime = [];

        //0. PROJECT EVALUATION
        var proj = EvalProject(root, target);
        procTime.Add(("~EVALUATION", sw.Elapsed.TotalMilliseconds));

        ShowProcessTime(procTime);

        VirtualDebugger.Debug(proj);

        /*
        //1. TOKENIZATION (LEXER)
        LexProject(proj);
        procTime.Add(("~LEXING", sw.Elapsed.TotalMilliseconds));

        //2. PARSING (PARSER)
        ParseProject(proj);
        procTime.Add(("~PARSING", sw.Elapsed.TotalMilliseconds));

        //3. LOWERING (LOWERER)
        LowerProject(proj);
        procTime.Add(("~LOWERING", sw.Elapsed.TotalMilliseconds));

        sw.Stop();
        ShowProcessTime(procTime);

        //DEBUG
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        opts.Converters.Add(new JsonStringEnumConverter());
        var json = File.ReadAllText(@"C:\Driza\DrizaSharp\dzdiag.config.json");
        var config = JsonSerializer.Deserialize<Diagnostics.Config>(json, opts);

        Debug(proj, config!);
        */
    }

    public static void ShowProcessTime(List<(string, double)> procTime)
    {
        double total = 0;
        foreach (var (name, time) in procTime)
        {
            Console.WriteLine($"{name} TIME: {time - total:F4}");
            total = time;
        }

        Console.WriteLine($"COMPILING TIME: {total:F4}");
    }
}