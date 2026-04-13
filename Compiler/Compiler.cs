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

        var lexer = Lexer.Manager.NewProcess(proj);
        var parser = Parser.Manager.NewProcess(proj);
        var lowerer = Lowerer.Manager.NewProcess(proj);

        void execPipeline()
        {
            bool flag;

            //01 LOADER
            flag = loader.Restore();
            procTime.Add(("[LOADER] RESTORE", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            flag = loader.Load();
            procTime.Add(("[LOADER] LOAD", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            //02 LEXER
            flag = lexer.Lex();
            procTime.Add(("[LEXER] LEX", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            //03 PARSER
            flag = parser.Match();
            procTime.Add(("[PARSER] MATCH", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            flag = parser.Bind();
            procTime.Add(("[PARSER] BIND", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            flag = parser.Validate();
            procTime.Add(("[PARSER] VALIDATE", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            flag = parser.Emit();
            procTime.Add(("[PARSER] EMIT", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;

            //04 LOWERER
            flag = lowerer.Lower();
            procTime.Add(("[LOWERER] LOWER", sw.Elapsed.TotalMilliseconds));
            if (!flag) return;
        }
        execPipeline();

        loader.EndProcess();
        lexer.EndProcess();
        parser.EndProcess();
        lowerer.EndProcess();

        //>>>> COMPILER TIME DISPLAY <<<<
        double total = 0;
        foreach (var (name, time) in procTime)
        {
            Console.WriteLine($"{name} TIME: {time - total:F4}");
            total = time;
        }

        Console.WriteLine($"COMPILING TIME: {total:F4}\n\n");

        //>>>> DEBUGGING <<<<
        Diagnostics.Manager.Debug(proj);

        /*
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        opts.Converters.Add(new JsonStringEnumConverter());
        var json = File.ReadAllText(@"C:\Driza\DrizaSharp\dzdiag.config.json");
        var config = JsonSerializer.Deserialize<Diagnostics.Config>(json, opts);
        */
    }
}