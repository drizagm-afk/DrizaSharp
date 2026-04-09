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
                Const.Rule_Int32(this);
                break;
            case InstrType.LdcInt64:
                Const.Rule_Int64(this);
                break;
            case InstrType.LdcFloat32:
                Const.Rule_Float32(this);
                break;
            case InstrType.LdcFloat64:
                Const.Rule_Float64(this);
                break;
            case InstrType.Ldstr:
                Const.Rule_String(this);
                break;
            case InstrType.Ldnull:
                Const.Rule_Null(this);
                break;

            //STACK
            case InstrType.Dup:
                Stack.Rule_Dup(this);
                break;

            case InstrType.Pop:
                Stack.Rule_Pop(this);
                break;

            //>>>> MATH <<<<
            //COMPARISON
            case InstrType.Equal:
                Compare.Rule_Equal(this);
                break;
            case InstrType.GreaterThan:
                Compare.Rule_GreaterThan(this);
                break;
            case InstrType.LessThan:
                Compare.Rule_GreaterThan(this);
                break;

            //ARITHMETIC
            case InstrType.Add:
                Arith.Rule_Add(this);
                break;
            case InstrType.Sub:
                Arith.Rule_Sub(this);
                break;
            case InstrType.Mul:
                Arith.Rule_Mul(this);
                break;
            case InstrType.Div:
                Arith.Rule_Div(this);
                break;
            case InstrType.Rem:
                Arith.Rule_Rem(this);
                break;

            //BITWISE
            case InstrType.And:
                Bitwise.Rule_And(this);
                break;
            case InstrType.Or:
                Bitwise.Rule_Or(this);
                break;
            case InstrType.Xor:
                Bitwise.Rule_Xor(this);
                break;
            case InstrType.Not:
                Bitwise.Rule_Not(this);
                break;
            case InstrType.ShiftLeft:
                Bitwise.Rule_ShiftLeft(this);
                break;
            case InstrType.ShiftRight:
                Bitwise.Rule_ShiftRight(this);
                break;

            //>>>> STORAGE <<<<
            //LOCALS
            case InstrType.DeclLocal:
                Local.Rule_Declare(this);
                break;
            case InstrType.LoadLocal:
                Local.Rule_Load(this);
                break;
            case InstrType.StoreLocal:
                Local.Rule_Store(this);
                break;

            //>>>> FLOW <<<<
            case InstrType.Return:
                Flow.Rule_Return(this);
                break;

            //BRANCHES
            case InstrType.Label:
                Branch.Rule_Label(this);
                break;
            case InstrType.Br:
                Branch.Rule_Goto(this);
                break;
            case InstrType.BrTrue:
                Branch.Rule_GotoIfTrue(this);
                break;
            case InstrType.BrFalse:
                Branch.Rule_GotoIfFalse(this);
                break;

            //>>>> TEMPORAL <<<<
            case InstrType.EnterMethod:
                Temporal.Rule_EnterMethod(this);
                break;
            case InstrType.Print:
                Temporal.Rule_Print(this);
                break;

            default:
                break;
        }
    }
}