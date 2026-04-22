using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    private partial bool LowerInstructions()
    {
        foreach (var file in Project.Files)
        {
            File = file;
            LowerRoot();
        }

        return !HasError();
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
            //>>>> CONST
            case InstrType.LoadInt32:
                InstrLoadInt32();
                break;
            case InstrType.LoadInt64:
                InstrLoadInt64();
                break;
            case InstrType.LoadFloat32:
                InstrLoadFloat32();
                break;
            case InstrType.LoadFloat64:
                InstrLoadFloat64();
                break;
            case InstrType.LoadString:
                InstrLoadString();
                break;
            case InstrType.LoadNull:
                InstrLoadNull();
                break;

            //>>>> FLOW
            case InstrType.EnterMethod:
                InstrEnterMethod();
                break;
            case InstrType.ExitMethod:
                InstrExitMethod();
                break;
            case InstrType.Return:
                InstrReturn();
                break;

            //BRANCHES
            case InstrType.Label:
                InstrLabel();
                break;
            case InstrType.Goto:
                InstrGoto();
                break;
            case InstrType.GotoIfTrue:
                InstrGotoIfTrue();
                break;
            case InstrType.GotoIfFalse:
                InstrGotoIfFalse();
                break;

            //>>>> MATH
            //ARITHMETIC
            case InstrType.Add:
                InstrAdd();
                break;
            case InstrType.Sub:
                InstrSub();
                break;
            case InstrType.Neg:
                InstrNeg();
                break;
            case InstrType.Mul:
                InstrMul();
                break;
            case InstrType.Div:
                InstrDiv();
                break;
            case InstrType.DivUnsigned:
                InstrDivUnsigned();
                break;
            case InstrType.Rem:
                InstrRem();
                break;
            case InstrType.RemUnsigned:
                InstrRemUnsigned();
                break;

            //BITWISE
            case InstrType.And:
                InstrAnd();
                break;
            case InstrType.Or:
                InstrOr();
                break;
            case InstrType.Xor:
                InstrXor();
                break;
            case InstrType.Not:
                InstrNot();
                break;
            case InstrType.ShiftLeft:
                InstrShiftLeft();
                break;
            case InstrType.ShiftRight:
                InstrShiftRight();
                break;

            //COMPARISON
            case InstrType.Equal:
                InstrEqual();
                break;
            case InstrType.GreaterThan:
                InstrGreaterThan();
                break;
            case InstrType.GreaterThanUnsigned:
                InstrGreaterThanUnsigned();
                break;
            case InstrType.LessThan:
                InstrLessThan();
                break;
            case InstrType.LessThanUnsigned:
                InstrLessThanUnsigned();
                break;

            //>>>> STACK
            case InstrType.Dup:
                InstrDup();
                break;
            case InstrType.Pop:
                InstrPop();
                break;

            //>>>> CALL
            case InstrType.Call:
                InstrCall();
                break;
            case InstrType.CallVirt:
                InstrCallVirt();
                break;
            case InstrType.NewObject:
                InstrNewObject();
                break;

            //>>>> TYPE
            //STRUCT
            case InstrType.Unbox:
                InstrUnbox();
                break;
            case InstrType.UnboxAddress:
                InstrUnboxAddress();
                break;
            case InstrType.Box:
                InstrBox();
                break;

            //ARRAY
            case InstrType.NewArray:
                InstrNewArray();
                break;
            case InstrType.LoadLength:
                InstrLoadLength();
                break;
            case InstrType.LoadElement:
                InstrLoadElement();
                break;
            case InstrType.LoadElementAddress:
                InstrLoadElementAddress();
                break;
            case InstrType.StoreElement:
                InstrStoreElement();
                break;

            //CAST
            case InstrType.CastTo:
                InstrCastTo();
                break;
            case InstrType.TryCastTo:
                InstrTryCastTo();
                break;

            //ADDRESS
            case InstrType.LoadFromAddress:
                InstrLoadFromAddress();
                break;
            case InstrType.StoreAtAddress:
                InstrStoreAtAddress();
                break;
            case InstrType.InitAtAddress:
                InstrInitAtAddress();

            //>>>> STORAGE
            //LOCALS
            case InstrType.LoadLocal:
                InstrLoadLocal();
                break;
            case InstrType.LoadLocalAddress:
                InstrLoadLocalAddress();
                break;
            case InstrType.StoreLocal:
                InstrStoreLocal();
                break;
            case InstrType.DeclLocal:
                InstrDeclLocal();
                break;

            //ARGS
            case InstrType.LoadArg:
                InstrLoadArg();
                break;
            case InstrType.LoadArgAddress:
                InstrLoadArgAddress();
                break;
            case InstrType.StoreArg:
                InstrStoreArg();
                break;

            //FIELDS
            case InstrType.LoadField:
                InstrLoadField();
                break;
            case InstrType.LoadFieldAddress:
                InstrLoadFieldAddress();
                break;
            case InstrType.StoreField:
                InstrStoreField();
                break;
        }
    }
}