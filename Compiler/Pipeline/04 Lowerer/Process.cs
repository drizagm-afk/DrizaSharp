using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public static class Manager
{
    //PROCESSES
    public static LowererProcess NewProcess(DzProject project) => new(project);
    public static void EndProcess(this LowererProcess process) { }
}
public partial class LowererProcess
{
    private DzProject Project;
    private ProjectDiagnostics GlobalDiagnostics => Project.LowererDiagnostics;

    internal LowererProcess(DzProject project)
    => Project = project;

    //>>>> PHASE CONTEXT <<<<
    private DzFile File = null!;

    private VIR VIR => Project.VIR;
    private CompilationContext CTX => CompilationContext.ContextAt(Project.Id);
    private TASI TASI => File.TASI;
    
    private FileDiagnostics<int> Diagnostics => File.LowererDiagnostics;

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
    internal AssemblyNameDefinition _asmName { get; private set; } = null!;
    internal AssemblyDefinition _asm => VIR.Definition;
    internal ModuleDefinition _module => VIR.Definition.MainModule;
    internal MethodDefinition _entryPoint { get; private set; } = null!;

    public bool Lower()
    {
        if (!CreateAssembly())
            return false;

        if (!LowerVirtual())
            return false;

        if (!LowerInstructions())
            return false;

        WriteAssembly();
        return true;
    }
    private bool CreateAssembly()
    {
        //>>>> CREATE ASSEMBLY
        _asmName = new("Program", new Version(1, 0, 0, 0));
        VIR.Definition = AssemblyDefinition.CreateAssembly(_asmName, "Program", new ModuleParameters()
        {
            AssemblyResolver = CompilationContext.Resolver,
            Kind = ModuleKind.Dll,
        });

        var corelib = _asm.MainModule.TypeSystem.CoreLibrary;
        if (corelib.Name != "System.Private.CoreLib")
        {
            GlobalDiagnostics.ReportUnhandled($"CORELIB ISN'T EXPECTED {CompilationContext.PATH_TO_CORELIB}: name={corelib.Name}");
            return false;
        }

        //>>>> CREATE ENTRYPOINT
        var entryType = new TypeDefinition(
            "Xx<_ENTRY_NSPACE_>xX", "Xx<_ENTRY_TYPE_>xX",
            TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract,
            REF_OBJECT
        );
        _module.Types.Add(entryType);

        _entryPoint = new MethodDefinition(
            "Xx<_ENTRYPOINT_>xX",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
            REF_VOID
        );
        entryType.Methods.Add(_entryPoint);
        _asm.EntryPoint = _entryPoint;

        return true;
    }
    private partial bool LowerVirtual();
    private partial bool LowerInstructions();
    private void WriteAssembly()
    {
        _asm.Write("Program.dll");
    }
}