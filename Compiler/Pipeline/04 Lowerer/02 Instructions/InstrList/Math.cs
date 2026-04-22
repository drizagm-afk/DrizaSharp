using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

//>>>> MATH ARITHMETIC <<<<
public static partial class InstrContext
{
    public static InstrType Add(this EmitContext _)
    => InstrType.Add;
    public static InstrType Sub(this EmitContext _)
    => InstrType.Sub;
    public static InstrType Mul(this EmitContext _)
    => InstrType.Mul;
    public static InstrType Div(this EmitContext _)
    => InstrType.Div;
    public static InstrType DivUnsigned(this EmitContext _)
    => InstrType.DivUnsigned;
    public static InstrType Rem(this EmitContext _)
    => InstrType.Rem;
    public static InstrType RemUnsigned(this EmitContext _)
    => InstrType.RemUnsigned;
}
public partial class LowererProcess
{
    private void NumAritmetic(string name, StackKind left, StackKind right)
    {
        if (!(left is StackKind.Int32 or StackKind.Int64 or StackKind.Float or StackKind.NativeInt))
            throw new AbortException($"Cannot perform {name} between \"{left}\" and \"{right}\"");

        if (left != right)
            throw new AbortException($"Cannot perform {name} between different numeric kinds, \"{left}\" and \"{right}\"");

        Push(ToUsage(left switch
        {
            StackKind.Int32 => CTX.TYPE_INT32,
            StackKind.Int64 => CTX.TYPE_INT64,
            StackKind.Float => CTX.TYPE_FLOAT64,
            _ => CTX.TYPE_INTPTR
        }));
    }
    private void NumAritmetic(string name, StackKind mono)
    {
        if (!(mono is StackKind.Int32 or StackKind.Int64 or StackKind.Float or StackKind.NativeInt))
            throw new AbortException($"Cannot perform {name} on \"{mono}\"");

        Push(ToUsage(mono switch
        {
            StackKind.Int32 => CTX.TYPE_INT32,
            StackKind.Int64 => CTX.TYPE_INT64,
            StackKind.Float => CTX.TYPE_FLOAT64,
            _ => CTX.TYPE_INTPTR
        }));
    }

    //MATH ARITHMETIC
    private void InstrAdd()
    {
        const string name = "ADD";
        PopTwice(name, out var a, out var b, out var aKind, out var bKind);
        if (aKind == StackKind.ByRef & bKind == StackKind.NativeInt)
            Push(a);
        else if (aKind == StackKind.NativeInt && bKind == StackKind.ByRef)
            Push(b);
        else
            NumAritmetic(name, aKind, bKind);

        _il.Append(_il.Create(OpCodes.Add));
    }
    private void InstrSub()
    {
        const string name = "SUB";
        PopTwice(name, out var a, out _, out var aKind, out var bKind);

        if (aKind == StackKind.ByRef && bKind == StackKind.NativeInt)
            Push(a);
        else if (aKind == StackKind.ByRef && bKind == StackKind.ByRef)
            Push(ToUsage(CTX.TYPE_INTPTR));
        else
            NumAritmetic(name, aKind, bKind);

        _il.Append(_il.Create(OpCodes.Sub));
    }
    private void InstrNeg()
    {
        const string name = "NEG";
        PopKindOnce(name, out var a);
        NumAritmetic(name, a);

        _il.Append(_il.Create(OpCodes.Neg));
    }
    private void InstrMul()
    {
        const string name = "MUL";
        PopKindTwice(name, out var a, out var b);
        NumAritmetic(name, a, b);

        _il.Append(_il.Create(OpCodes.Mul));
    }
    private void InstrDiv()
    {
        const string name = "DIV";
        PopKindTwice(name, out var a, out var b);
        NumAritmetic(name, a, b);

        _il.Append(_il.Create(OpCodes.Div));
    }
    private void InstrDivUnsigned()
    {
        const string name = "DIV UNSIGNED";
        PopKindTwice(name, out var a, out var b);
        NumAritmetic(name, a, b);

        _il.Append(_il.Create(OpCodes.Div_Un));
    }
    private void InstrRem()
    {
        const string name = "REM";
        PopKindTwice(name, out var a, out var b);
        NumAritmetic(name, a, b);

        _il.Append(_il.Create(OpCodes.Rem));
    }
    private void InstrRemUnsigned()
    {
        const string name = "REM UNSIGNED";
        PopKindTwice(name, out var a, out var b);
        NumAritmetic(name, a, b);

        _il.Append(_il.Create(OpCodes.Rem_Un));
    }
}

//>>>> MATH BITWISE <<<<
public static partial class InstrContext
{
    public static InstrType And(this EmitContext _)
    => InstrType.And;
    public static InstrType Or(this EmitContext _)
    => InstrType.Or;
    public static InstrType Xor(this EmitContext _)
    => InstrType.Xor;
    public static InstrType Not(this EmitContext _)
    => InstrType.Not;
    public static InstrType ShiftLeft(this EmitContext _)
    => InstrType.ShiftLeft;
    public static InstrType ShiftRight(this EmitContext _)
    => InstrType.ShiftRight;
}
public partial class LowererProcess
{
    private void NumBitwise(string name, StackKind left, StackKind right)
    {
        if (!(left is StackKind.Int32 or StackKind.Int64 or StackKind.NativeInt))
            throw new AbortException($"Cannot perform {name} between \"{left}\" and \"{right}\"");

        if (left != right)
            throw new AbortException($"Cannot perform {name} between different numeric kinds, \"{left}\" and \"{right}\"");

        Push(ToUsage(left switch
        {
            StackKind.Int32 => CTX.TYPE_INT32,
            StackKind.Int64 => CTX.TYPE_INT64,
            _ => CTX.TYPE_INTPTR
        }));
    }
    private void NumBitwise(string name, StackKind mono)
    {
        if (!(mono is StackKind.Int32 or StackKind.Int64 or StackKind.NativeInt))
            throw new AbortException($"Cannot perform {name} on \"{mono}\"");

        Push(ToUsage(mono switch
        {
            StackKind.Int32 => CTX.TYPE_INT32,
            StackKind.Int64 => CTX.TYPE_INT64,
            _ => CTX.TYPE_INTPTR
        }));
    }
    private void ShiftBitwise(string name, StackKind left, StackKind right)
    {
        if (!(left is StackKind.Int32 or StackKind.Int64 or StackKind.NativeInt) || right != StackKind.Int32)
            throw new AbortException($"Cannot perform {name} between \"{left}\" and \"{right}\"");

        Push(ToUsage(left switch
        {
            StackKind.Int32 => CTX.TYPE_INT32,
            StackKind.Int64 => CTX.TYPE_INT64,
            _ => CTX.TYPE_INTPTR
        }));
    }

    //MATH BITWISE
    private void InstrAnd()
    {
        const string name = "AND";
        PopKindTwice(name, out var a, out var b);
        NumBitwise(name, a, b);

        _il.Append(_il.Create(OpCodes.And));
    }
    private void InstrOr()
    {
        const string name = "OR";
        PopKindTwice(name, out var a, out var b);
        NumBitwise(name, a, b);

        _il.Append(_il.Create(OpCodes.Or));
    }
    private void InstrXor()
    {
        const string name = "XOR";
        PopKindTwice(name, out var a, out var b);
        NumBitwise(name, a, b);

        _il.Append(_il.Create(OpCodes.Xor));
    }
    private void InstrNot()
    {
        const string name = "NOT";
        PopKindOnce(name, out var a);
        NumBitwise(name, a);

        _il.Append(_il.Create(OpCodes.Not));
    }
    private void InstrShiftLeft()
    {
        const string name = "SHIFT LEFT";
        PopKindTwice(name, out var a, out var b);
        ShiftBitwise(name, a, b);

        _il.Append(_il.Create(OpCodes.Shl));
    }
    private void InstrShiftRight()
    {
        const string name = "SHIFT RIGHT";
        PopKindTwice(name, out var a, out var b);
        ShiftBitwise(name, a, b);

        _il.Append(_il.Create(OpCodes.Shr));
    }
}

//>>>> MATH COMPARE <<<<
public static partial class InstrContext
{
    public static InstrType Equal(this EmitContext _)
    => InstrType.Equal;
    public static InstrType GreaterThan(this EmitContext _)
    => InstrType.GreaterThan;
    public static InstrType GreaterThanUnsigned(this EmitContext _)
    => InstrType.GreaterThanUnsigned;
    public static InstrType LessThan(this EmitContext _)
    => InstrType.LessThan;
    public static InstrType LessThanUnsigned(this EmitContext _)
    => InstrType.LessThanUnsigned;
}
public partial class LowererProcess
{
    private void IdCompare(string name, StackKind left, StackKind right)
    {
        if (left != right)
            throw new AbortException($"Cannot perform {name} between different kinds, \"{left}\" and \"{right}\"");

        Push(ToUsage(CTX.TYPE_INT32));
    }
    private void NumCompare(string name, StackKind left, StackKind right)
    {
        if (!(left is StackKind.Int32 or StackKind.Int64 or StackKind.Float or StackKind.NativeInt))
            throw new AbortException($"Cannot perform {name} between \"{left}\" and \"{right}\"");

        if (left != right)
            throw new AbortException($"Cannot perform {name} between different kinds, \"{left}\" and \"{right}\"");

        Push(ToUsage(CTX.TYPE_INT32));
    }

    //MATH COMPARE
    private void InstrEqual()
    {
        const string name = "COMPARE EQUAL";
        PopKindTwice(name, out var a, out var b);
        IdCompare(name, a, b);

        _il.Append(_il.Create(OpCodes.Ceq));
    }
    private void InstrGreaterThan()
    {
        const string name = "COMPARE GREATER THAN";
        PopKindTwice(name, out var a, out var b);
        NumCompare(name, a, b);

        _il.Append(_il.Create(OpCodes.Cgt));
    }
    private void InstrGreaterThanUnsigned()
    {
        const string name = "COMPARE GREATER THAN UNSIGNED";
        PopKindTwice(name, out var a, out var b);
        NumCompare(name, a, b);

        _il.Append(_il.Create(OpCodes.Cgt_Un));
    }
    private void InstrLessThan()
    {
        const string name = "COMPARE LESS THAN";
        PopKindTwice(name, out var a, out var b);
        NumCompare(name, a, b);

        _il.Append(_il.Create(OpCodes.Clt));
    }
    private void InstrLessThanUnsigned()
    {
        const string name = "COMPARE LESS THAN UNSIGNED";
        PopKindTwice(name, out var a, out var b);
        NumCompare(name, a, b);

        _il.Append(_il.Create(OpCodes.Clt_Un));
    }
}