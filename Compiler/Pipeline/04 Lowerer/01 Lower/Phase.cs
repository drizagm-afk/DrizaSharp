using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    //>>>> LOWER PROJECT <<<<
    public partial void Lower()
    {
        CreateAssembly();

        LowerVirtual();
        LowerInstructions();

        WriteAssembly();
    }
    private void CreateAssembly()
    {
        _asmName = new("Program", new Version(1, 0, 0, 0));
        _asm = AssemblyDefinition.CreateAssembly(_asmName, "Program", ModuleKind.Dll);
    }
    private void WriteAssembly()
    {
        _asm.Write("Program.dll");
    }
}