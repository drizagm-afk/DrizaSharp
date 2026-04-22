using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace DrzSharp.Compiler.Lowerer;

//>>>> METHOD BODY <<<<
public partial class LowererProcess
{
    private HashSet<int> _initMethods = [];

    private VCollection<VMethodParam> _params = null!;
    private UType _return = null!;

    private MethodBody _methodBody = null!;
    private ILProcessor _il => _methodBody.GetILProcessor();
    private void EnterMethod(int methodId)
    {
        if (methodId < 0)
        {
            _params = null!;
            _return = ToUsage(CTX.TYPE_VOID);
            _methodBody = _entryPoint.Body;
        }
        else
        {
            var method = VIR.EditAt<VMethodMemberEdit>(methodId);
            _params = method.Params;
            _methodBody = method.Definition.Body;
        }

        if (_initMethods.Add(methodId))
            _methodBody.InitLocals = true;
    }
    private void ExitMethod()
    {
        _labels.Clear();
        _stack.Clear();
    }

    //LABELS
    private List<(Instruction def, bool isDecl)> _labels = [];
    private (Instruction def, bool isDecl) EnsureLabel(int labelId)
    {
        while (_labels.Count <= labelId)
            _labels.Add((_il.Create(OpCodes.Nop), false));

        return _labels[labelId];
    }

    //LOCALS
    private Collection<VariableDefinition> _locals => _methodBody.Variables;

    //STACK
    private Stack<UType> _stack = [];
    private bool Peek(out UType utype)
    => _stack.TryPeek(out utype!);
    private bool Pop(out UType utype)
    => _stack.TryPop(out utype!);
    private void Push(UType utype)
    => _stack.Push(ToStackType(utype));

    private void PopOnce(string name, out UType mono)
    {
        if (!Pop(out mono))
            throw new AbortException($"The Stack MUST have at least one value WHEN performing {name}");
    }
    private void PopOnce(string name, out UType mono, out StackKind monoKind)
    {
        PopOnce(name, out mono);
        monoKind = StackKindOf(mono);
    }
    private void PopKindOnce(string name, out StackKind mono)
    => PopOnce(name, out _, out mono);

    private void PopTwice(string name, out UType left, out UType right)
    {
        if (!Pop(out right) || !Pop(out left))
            throw new AbortException($"The Stack MUST have at least two values WHEN performing {name}");
    }
    private void PopTwice(string name, out UType left, out UType right, out StackKind leftKind, out StackKind rightKind)
    {
        PopTwice(name, out left, out right);
        leftKind = StackKindOf(left);
        rightKind = StackKindOf(right);
    }
    private void PopKindTwice(string name, out StackKind left, out StackKind right)
    => PopTwice(name, out _, out _, out left, out right);


    //STACK TYPES
    private static UType ToUsage(VType type)
    => UContext.GetDeclType(type.GlobalId);

    private UType ToStackType(VType type)
    => ToStackType(ToUsage(type));
    private UType ToStackType(UType utype)
    => ToStackType(CTX, utype);
    internal static UType ToStackType(CompilationContext ctx, UType utype)
    {
        if (utype == ToUsage(ctx.TYPE_UINT32)
        || utype == ToUsage(ctx.TYPE_INT16)
        || utype == ToUsage(ctx.TYPE_UINT16)
        || utype == ToUsage(ctx.TYPE_INT8)
        || utype == ToUsage(ctx.TYPE_UINT8)
        || utype == ToUsage(ctx.TYPE_CHAR)
        || utype == ToUsage(ctx.TYPE_BOOL))
            return ToUsage(ctx.TYPE_INT32);
        else if (utype == ToUsage(ctx.TYPE_UINT64))
            return ToUsage(ctx.TYPE_INT64);
        else if (utype == ToUsage(ctx.TYPE_FLOAT32))
            return ToUsage(ctx.TYPE_FLOAT64);
        else if (utype == ToUsage(ctx.TYPE_UINTPTR) || utype is UPointerType)
            return ToUsage(ctx.TYPE_INTPTR);

        return utype;
    }

    private StackKind StackKindOf(UType utype)
    => StackKindOf(CTX, utype);
    internal static StackKind StackKindOf(CompilationContext ctx, UType utype)
    {
        if (utype == ToUsage(ctx.TYPE_INT32))
            return StackKind.Int32;
        if (utype == ToUsage(ctx.TYPE_INT64))
            return StackKind.Int64;
        if (utype == ToUsage(ctx.TYPE_FLOAT64))
            return StackKind.Float;
        if (utype == ToUsage(ctx.TYPE_INTPTR))
            return StackKind.Pointer;
        if (utype is UAddressType)
            return StackKind.Address;

        return StackKind.Object;
    }
}
public enum StackKind
{ Int32, Int64, Float, Pointer, Object, Address }

//>>>> READ <<<<
public partial class LowererProcess
{
    private Instr Instruction;

    private int _offset;
    private byte ReadByte()
    {
        var val = TASI.ReadByte(_offset);
        _offset += TASI.BYTE_SIZE;
        return val;
    }
    private int ReadInt32()
    {
        var val = TASI.ReadInt32(_offset);
        _offset += TASI.INT32_SIZE;
        return val;
    }
    private long ReadInt64()
    {
        var val = TASI.ReadInt64(_offset);
        _offset += TASI.INT64_SIZE;
        return val;
    }
    private float ReadFloat32()
    {
        var val = TASI.ReadFloat32(_offset);
        _offset += TASI.FLOAT32_SIZE;
        return val;
    }
    private double ReadFloat64()
    {
        var val = TASI.ReadFloat64(_offset);
        _offset += TASI.FLOAT64_SIZE;
        return val;
    }

    private T ReadObject<T>()
    {
        var val = TASI.ReadObject(_offset);
        _offset += TASI.REF_SIZE;
        return (T)val;
    }
    private string ReadString()
    {
        var val = TASI.ReadString(_offset);
        _offset += TASI.REF_SIZE;
        return val;
    }
}