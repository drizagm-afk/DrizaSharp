using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    private void LowerInstructions()
    {
        foreach (var file in Project.Files)
        {
            File = file;
            LowerRoot();
        }
    }
    private void LowerRoot()
    {
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
        //STACK CHILDREN
        Stack<(int nodeId, int relIndex)> children = [];
        var childExists = TASI.TryNodeAt(node.FirstChildId, out var child);
        while (childExists)
        {
            children.Push(new(child.Id, child.RelIndex));
            childExists = TASI.TryNodeAt(child.NextSiblingId, out child);
        }

        //LOWER INSTRUCTIONS
        for (int i = 0; i < node.Length; i++)
        {
            LowerInstr(TASI.InstructionAt(node.Start + i));

            while (children.TryPeek(out var next) && next.relIndex == i)
            {
                children.Pop();
                LowerNode(TASI.NodeAt(next.nodeId));
            }
        }
    }
    private void LowerInstr(Instr instr)
    {
        Instruction = instr;
        _offset = instr.Start;

        switch (instr.Type)
        {
            //CONSTANTS
            case InstrType.LdcInt32:
                Constant.Rule_Int32(this);
                break;
            case InstrType.LdcInt64:
                Constant.Rule_Int64(this);
                break;
            case InstrType.LdcFloat32:
                Constant.Rule_Float32(this);
                break;
            case InstrType.LdcFloat64:
                Constant.Rule_Float64(this);
                break;
            case InstrType.Ldstr:
                Constant.Rule_String(this);
                break;

            //FLOW
            case InstrType.Label:
                Branches.Rule_Label(this);
                break;
            case InstrType.Br:
                Branches.Rule_Br(this);
                break;
            case InstrType.BrTrue:
                Branches.Rule_BrIfTrue(this);
                break;
            case InstrType.BrFalse:
                Branches.Rule_BrIfFalse(this);
                break;

            //MATH
            case InstrType.Equal:
                Compare.Rule_Equal(this);
                break;
            case InstrType.GreaterThan:
                Compare.Rule_GreaterThan(this);
                break;
            case InstrType.LessThan:
                Compare.Rule_GreaterThan(this);
                break;

            case InstrType.Add:
                Arithmetic.Rule_Add(this);
                break;
            case InstrType.Sub:
                Arithmetic.Rule_Sub(this);
                break;
            case InstrType.Mul:
                Arithmetic.Rule_Mul(this);
                break;
            case InstrType.Div:
                Arithmetic.Rule_Div(this);
                break;

            //MEMORY
            case InstrType.Local:
                Locals.Rule_DeclLocal(this);
                break;
            case InstrType.LoadLocal:
                Locals.Rule_LoadLocal(this);
                break;
            case InstrType.StoreLocal:
                Locals.Rule_StoreLocal(this);
                break;

            //SPECIAL
            case InstrType.EnterMethod:
                Special.Rule_EnterMethod(this);
                break;
            case InstrType.Print:
                Special.Rule_Print(this);
                break;
            case InstrType.Return:
                Special.Rule_Return(this);
                break;
        }
    }
}