using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Project;
using Mono.Cecil;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    //LOWER PROJECT
    private AssemblyNameDefinition _asmName = null!;
    private AssemblyDefinition _asm = null!;

    public void LowerProject(DzProject project)
    {
        //BUILD ASSEMBLY
        _asmName = new("Program", new Version(1, 0, 0, 0));
        _asm = AssemblyDefinition.CreateAssembly(_asmName, "Program", ModuleKind.Dll);

        Module = _asm.MainModule;

        //EXECUTE INSTRUCTIONS
        foreach (var file in project.Files) LowerFile(file);

        //FINALIZE
        _asm.Write("Program.dll");
        Reset();
    }
    public void Reset()
    {
        _asm = null!;
        File = null!;

        ResetVirtual();
        ResetLogic();
    }

    //LOWER FILE
    private DzFile File = null!;
    private TASI TASI => File.TASI;
    private GroupDiagnostics Diagnostics => File.Diagnostics.Lowerer;

    private void LowerFile(DzFile file)
    {
        File = file;
        //LOWER ROOT
        void lowerSibs(int nodeId)
        {
            if (TASI.TryNodeAt(nodeId, out var child))
            {
                lowerSibs(child.NextSiblingId);
                LowerNode(child);
            }
        }
        lowerSibs(TASI.Root.FirstChildId);
    }
    private void LowerNode(in TASINode node)
    {
        //STACKING CHILDREN
        Stack<NodeRef> children = [];
        var childExists = TASI.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            children.Push(new(child.Id, child.RelIndex));
            childExists = TASI.TryNodeAt(child.NextSiblingId, out child);
        }

        //LOWERING
        for (int i = 0; i < node.Length; i++)
        {
            LowerInst(TASI.InstructionAt(node.Start + i), TASI.InfoAt(node.Id).SourceId);

            while (children.TryPeek(out var next) && next.RelIndex == i)
            {
                children.Pop();
                LowerNode(TASI.NodeAt(next.NodeId));
            }
        }
    }
    private readonly struct NodeRef(int nodeId, int relIndex)
    {
        public readonly int NodeId = nodeId;
        public readonly int RelIndex = relIndex;
    }

    private void LowerInst(Instruction inst, int sourceId)
    {
        if (LowererManager.TryGetRule(inst.RuleId, out var rule))
            rule(this, inst);
        else
            Diagnostics.ReportUnexpected(inst.Source, ParserManager.GetRuleName(sourceId));
    }
}