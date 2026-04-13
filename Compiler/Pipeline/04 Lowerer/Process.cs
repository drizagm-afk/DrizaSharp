using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public delegate void Rule(Context ctx);
public static class Manager
{
    //PROCESSES
    public static LowererProcess NewProcess(DzProject project) => new(project);
    public static void EndProcess(this LowererProcess process) { }
}
public partial class LowererProcess
{
    public DzProject Project { get; internal set; }
    private ProjectDiagnostics GlobalDiagnostics => Project.LowererDiagnostics;

    internal LowererProcess(DzProject project)
    => Project = project;

    //>>>> PHASE CONTEXT <<<<
    private DzFile File = null!;
    private TASI TASI => File.TASI;
    private FileDiagnostics<int> Diagnostics => File.LowererDiagnostics;

    private AssemblyNameDefinition _asmName = null!;
    private AssemblyDefinition _asm = null!;

    //>>>> DEBUG <<<<
    public bool HasError()
    {
        if (Project.LowererDiagnostics.HasError)
            return true;
        
        foreach (var file in Project.Files)
        {
            if (file.LowererDiagnostics.HasError)
                return true;
        }
        return false;
    }

    //>>>> PHASES <<<<
    public partial bool Lower();
}