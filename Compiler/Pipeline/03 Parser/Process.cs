using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Parser;

namespace DrzSharp.Compiler.Parser;

public static class Manager
{
    //PROCESSES
    public static ParserProcess NewProcess(DzProject project) => new(project);
    public static void EndProcess(this ParserProcess process) { }
}
public partial class ParserProcess
{
    public DzProject Project { get; internal set; }
    
    private VIR VIR => Project.VIR;
    private ProjectDiagnostics GlobalDiagnostics => Project.ParserDiagnostics;

    internal ParserProcess(DzProject project)
    => Project = project;

    //>>>> PHASE CONTEXT <<<<
    private DzFile File = null!;
    private DzModule Module => Project.Modules[File.ModuleId];
        
    private TAST TAST => File.TAST;
    private TASI TASI => File.TASI;
    private FileDiagnostics<int> Diagnostics => File.ParserDiagnostics;

    private RuleInstance? RuleInst;

    //>>>> PHASES <<<<
    public partial void Match();
    public partial void Bind();
    public partial void Validate();
    public partial void Emit();
}