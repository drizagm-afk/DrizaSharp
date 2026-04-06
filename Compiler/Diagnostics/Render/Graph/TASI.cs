using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    private void DebugTASI()
    {
        PrintSectionHeader("LOWERER");
        WriteLine(">> TASI (Abstract Stratified Instruction Tree): ");

        int tabs = 0;
        PrintGConn("VIRTUAL ROOT", tabs);
        void printSibs(int nodeId)
        {
            if (TASI.TryNodeAt(nodeId, out var child))
            {
                printSibs(child.NextSiblingId);
                DebugTASI(child, ref tabs);
            }
        }
        printSibs(TASI.Root.FirstChildId);
        WriteLine();
    }
    private void DebugTASI(in TASINode node, ref int tabs)
    {
        tabs++;

        //HEADER
        var source = TASI.InfoAt(node.Id).SourceNodeId;
        PrintGConn($"From <{source:D3}> {Rules.Parser.RuleExt.GetRule(Project, TAST.GetApplyRule(source).RuleId).Name}", tabs);

        //STACKING CHILDREN
        Stack<(int nodeId, int relIndex)> children = [];
        var childExists = TASI.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            children.Push(new(child.Id, child.RelIndex));
            childExists = TASI.TryNodeAt(child.NextSiblingId, out child);
        }

        //PRINTING
        for (int i = 0; i < node.Length; i++)
        {
            var instr = TASI.InstructionAt(node.Start + i);
            PrintGTab($"[{i}] {instr.Type}", tabs);

            while (children.TryPeek(out var next) && next.relIndex == i)
            {
                children.Pop();
                DebugTASI(TASI.NodeAt(next.nodeId), ref tabs);
            }
        }

        tabs--;
    }
}