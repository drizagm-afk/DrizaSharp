using System.Diagnostics;
using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Loader;
using DrzSharp.Compiler.Lowerer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler;

public static partial class Compiler
{
    public static void Compile(string path)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<(string, double)> procTime = [];

        //1 LOADER
        var loader = Loader.Manager.NewProcess(path);
        loader.Restore();
        procTime.Add(("[LOADER] RESTORE", sw.Elapsed.TotalMilliseconds));

        loader.Load();
        procTime.Add(("[LOADER] LOAD", sw.Elapsed.TotalMilliseconds));

        loader.EndProcess();

        //2 LEXER
        var lexer = Lexer.Manager.NewProcess(loader.Project);
        lexer.Lex();
        procTime.Add(("[LEXER] LEX", sw.Elapsed.TotalMilliseconds));

        lexer.EndProcess();

        //3 PARSER
        var parser = Parser.Manager.NewProcess(loader.Project);
        parser.Match();
        procTime.Add(("[PARSER] MATCH", sw.Elapsed.TotalMilliseconds));

        parser.Bind();
        procTime.Add(("[PARSER] BIND", sw.Elapsed.TotalMilliseconds));

        parser.Validate();
        procTime.Add(("[PARSER] VALIDATE", sw.Elapsed.TotalMilliseconds));

        parser.Emit();
        procTime.Add(("[PARSER] EMIT", sw.Elapsed.TotalMilliseconds));

        parser.EndProcess();

        //4 LOWERER
        var lowerer = Lowerer.Manager.NewProcess(loader.Project);
        lowerer.Lower();
        procTime.Add(("[LOWERER] LOWER", sw.Elapsed.TotalMilliseconds));

        lowerer.EndProcess();

        ShowProcessTime(procTime);

        /*
        VirtualDebugger.Debug(loader.Project.Assembly.Virtual);

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