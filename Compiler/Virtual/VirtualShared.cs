using System.Reflection;
using System.Reflection.Metadata;

namespace DrzSharp.Compiler.Virtual;

public static class InstanceKind
{
    public const byte Type = 1;
    public const byte Method = 2;
    public const byte Field = 3;

    public static bool IsType(this VirtualInstance inst) => inst.Kind == Type;
    public static bool IsMethod(this VirtualInstance inst) => inst.Kind == Method;
    public static bool IsField(this VirtualInstance inst) => inst.Kind == Field;
}
public readonly record struct TypeArgs (
    TypeAttributes Attributes,
    int GenericArgs
) : InstanceArgs;
public readonly record struct MethodArgs(
    MethodAttributes Attributes,
    BlobHandle Signature,
    int GenericArgs
) : InstanceArgs;
public readonly record struct FieldArgs(
    FieldAttributes Attributes,
    BlobHandle Signature
) : InstanceArgs;