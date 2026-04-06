using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lexer;

public static class Manager
{
    //PROCESSES
    public static LexerProcess NewProcess(DzProject project) => new(project);
    public static void EndProcess(this LexerProcess process) { }
}
public partial class LexerProcess
{
    public DzProject Project { get; internal set; }

    internal LexerProcess(DzProject project)
    => Project = project;

    //>>>> PHASE CONTEXT <<<<
    private DzFile File = null!;
    private DzModule Module => Project.Modules[File.ModuleId];

    private SourceText Source => File.Source;
    private TAST TAST => File.TAST;
    private FileDiagnostics<RuleId> Diagnostics => File.LexerDiagnostics;

    //>>>> PHASES <<<<
    public partial void Lex();
}