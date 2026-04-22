using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> FLOW <<<<
public static partial class InstrContext
{
    public static InstrType EnterMethod(this EmitContext ctx, int methodId)
    {
        ((EmitInstrContext)ctx).WriteInt32(methodId);
        return InstrType.EnterMethod;
    }
    public static InstrType ExitMethod(this EmitContext _)
    => InstrType.ExitMethod;
    public static InstrType Return(this EmitContext _)
    => InstrType.Return;
}
public partial class LowererProcess
{
    private void InstrEnterMethod()
    => EnterMethod(ReadInt32());
    private void InstrExitMethod()
    {
        //ENSURE LABEL DECLARATION
        for (int i = 0; i < _labels.Count; i++)
            if (_labels[i].isDecl)
                throw new AbortException($"LABEL_{i} is used but never Declared");
        
        //ENSURE ALL PATHS DO RETURN

        ExitMethod();
    }
    private void InstrReturn()
    {
        if (_return != ToUsage(CTX.TYPE_VOID))
        {
            if (!Pop(out var retType))
                throw new AbortException($"The Stack MUST have a value WHEN returning a NON-VOID method");
            if (ToStackType(_return) != retType)
                throw new AbortException($"Returned \"{retType}\", while expected RETURN TYPE is \"{_return}\"");
        }
        if (_stack.Count > 0)
            throw new AbortException("The stack must be EMPTY after return");

        _il.Append(_il.Create(OpCodes.Ret));
    }
}

//>>>> FLOW BRANCH <<<<
public static partial class InstrContext
{
    public static InstrType Label(this EmitContext ctx, int labelId)
    {
        ((EmitInstrContext)ctx).WriteInt32(labelId);
        return InstrType.Label;
    }
    public static InstrType Goto(this EmitContext ctx, int labelId)
    {
        ((EmitInstrContext)ctx).WriteInt32(labelId);
        return InstrType.Goto;
    }
    public static InstrType GotoIfTrue(this EmitContext ctx, int labelId)
    {
        ((EmitInstrContext)ctx).WriteInt32(labelId);
        return InstrType.GotoIfTrue;
    }
    public static InstrType GotoIfFalse(this EmitContext ctx, int labelId)
    {
        ((EmitInstrContext)ctx).WriteInt32(labelId);
        return InstrType.GotoIfFalse;
    }
}
public partial class LowererProcess
{
    private void InstrLabel()
    {
        var id = ReadInt32();
        if (_stack.Count > 0)
            throw new AbortException($"The Stack MUST be empty BEFORE declaring LABEL_{id}");

        var label = EnsureLabel(id);
        if (label.isDecl)
            throw new AbortException($"Tried to Declare LABEL_{id} more than once");
        label.isDecl = true;

        _labels[id] = label;
        _il.Append(label.def);
    }
    private void InstrGoto()
    {
        var id = ReadInt32();
        if (_stack.Count > 0)
            throw new AbortException($"The Stack MUST be empty AFTER Goto, LABEL_{id}");

        (var def, _) = EnsureLabel(id);
        _il.Append(_il.Create(OpCodes.Br, def));
    }
    private void InstrGotoIfTrue()
    {
        var id = ReadInt32();
        if (!Pop(out _))
            throw new AbortException($"The Stack MUST have a value WHEN GotoIfTrue, LABEL_{id}");
        if (_stack.Count > 0)
            throw new AbortException($"The Stack MUST be empty AFTER GotoIfTrue, LABEL_{id}");

        (var def, _) = EnsureLabel(id);
        _il.Append(_il.Create(OpCodes.Brtrue, def));
    }
    private void InstrGotoIfFalse()
    {
        var id = ReadInt32();
        if (!Pop(out _))
            throw new AbortException($"The Stack MUST have a value WHEN GotoIfFalse, LABEL_{id}");
        if (_stack.Count > 0)
            throw new AbortException($"The Stack MUST be empty AFTER GotoIfFalse, LABEL_{id}");

        (var def, _) = EnsureLabel(id);
        _il.Append(_il.Create(OpCodes.Brfalse, def));
    }
}