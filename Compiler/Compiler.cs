using System.Diagnostics;
using DrzSharp.Compiler.Diagnostics;
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

        var loader = Loader.Manager.NewProcess(path);
        var proj = loader.Project;

        void execPipeline()
        {
            //01 LOADER
            loader.Restore();
            procTime.Add(("[LOADER] RESTORE", sw.Elapsed.TotalMilliseconds));

            loader.Load();
            procTime.Add(("[LOADER] LOAD", sw.Elapsed.TotalMilliseconds));

            //02 LEXER
            var lexer = Lexer.Manager.NewProcess(proj);
            lexer.Lex();
            procTime.Add(("[LEXER] LEX", sw.Elapsed.TotalMilliseconds));

            lexer.EndProcess();

            //03 PARSER
            var parser = Parser.Manager.NewProcess(proj);
            parser.Match();
            procTime.Add(("[PARSER] MATCH", sw.Elapsed.TotalMilliseconds));

            parser.Bind();
            procTime.Add(("[PARSER] BIND", sw.Elapsed.TotalMilliseconds));

            parser.Validate();
            procTime.Add(("[PARSER] VALIDATE", sw.Elapsed.TotalMilliseconds));

            if (proj.HasError()) return;

            parser.Emit();
            procTime.Add(("[PARSER] EMIT", sw.Elapsed.TotalMilliseconds));

            parser.EndProcess();

            //04 LOWERER
            var lowerer = Lowerer.Manager.NewProcess(proj);
            lowerer.Lower();
            procTime.Add(("[LOWERER] LOWER", sw.Elapsed.TotalMilliseconds));

            lowerer.EndProcess();
        }
        execPipeline();

        loader.EndProcess();

        //>>>> DEBUGGING <<<<
        ShowProcessTime(procTime);

        Diagnostics.Manager.Debug(proj);

        /*
        VirtualDebugger.Debug(proj.Assembly.Virtual);

        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        opts.Converters.Add(new JsonStringEnumConverter());
        var json = File.ReadAllText(@"C:\Driza\DrizaSharp\dzdiag.config.json");
        var config = JsonSerializer.Deserialize<Diagnostics.Config>(json, opts);
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

        Console.WriteLine($"COMPILING TIME: {total:F4}\n\n");
    }
}