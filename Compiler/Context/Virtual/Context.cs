using System.Buffers.Binary;
using System.Security.Cryptography;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    //>>>> STATIC: CACHED ASSEMBLIES <<<<
    private static readonly List<VAssemblyEdit> _assemblies = [];
    public static IEnumerable<VAssemblyEdit> Assemblies => _assemblies;
    public static VAssemblyEdit AssemblyAt(int id)
    => _assemblies[id];
    private static VAssemblyEdit AddAssembly(AssemblyDefinition definition, AssemblyHash hash)
    {
        var asmId = _assemblies.Count;
        var vasm = new VAssemblyEdit(asmId) { Definition = definition };

        _assemblies.Add(vasm);
        _assembliesByHash[hash] = asmId;

        return vasm;
    }

    private static readonly Dictionary<AssemblyHash, int> _assembliesByHash = [];
    private static bool TryGetLoadedAssembly(AssemblyHash hash, out VAssemblyEdit vasm)
    {
        vasm = null!;
        if (!_assembliesByHash.TryGetValue(hash, out int asmId))
            return false;

        vasm = AssemblyAt(asmId);
        return true;
    }

    //>>>> DEPENDENCIES <<<<
    private readonly List<int> _deps = [];
    public IEnumerable<VAssemblyEdit> Dependencies
    {
        get
        {
            foreach (var dep in _deps)
                yield return AssemblyAt(dep);
        }
    }
    public VAssemblyEdit DependencyAt(int id)
    => AssemblyAt(_deps[id]);
    private int AddDependency(VAssemblyEdit vasm)
    {
        _deps.Add(vasm.Id);
        _depsByName[vasm.Definition.DependencyName()] = vasm.Id;

        return vasm.Id;
    }
    private void RemoveLastAssemblyAndDependency()
    {
        var vasm = _assemblies.RemoveLast();
        _assembliesByHash.Remove(vasm.Hash);

        _deps.RemoveLast();
        _depsByName.Remove(vasm.Definition.DependencyName());
    }

    private readonly Dictionary<DependencyName, int> _depsByName = [];
    internal VAssemblyEdit GetDependency(DependencyName name)
    => AssemblyAt(_depsByName[name]);
    internal bool HasDependency(DependencyName name)
    => _depsByName.ContainsKey(name);
    internal bool TryGetDependency(DependencyName name, out VAssemblyEdit vasm)
    {
        vasm = null!;
        if (!_depsByName.TryGetValue(name, out int asmId))
            return false;

        vasm = AssemblyAt(asmId);
        return true;
    }
}

public readonly record struct AssemblyHash
{
    private readonly string _hash;

    internal AssemblyHash(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);

        var hash = sha.ComputeHash(stream);
        _hash = Convert.ToHexString(hash);
    }
    public override string ToString() => _hash;
}
public static class AssemblyExt
{
    internal static DependencyName DependencyName(this AssemblyDefinition asm)
    => asm.Name.DependencyName();
    internal static DependencyName DependencyName(this AssemblyNameDefinition asm)
    => new(asm.Name, asm.PublicKeyToken);
    internal static DependencyName DependencyName(this AssemblyNameReference asm)
    => new(asm.Name, asm.PublicKeyToken);
}
public readonly record struct DependencyName
{
    private readonly string _name;
    private readonly ulong _publicKeyToken;

    internal DependencyName(string name, byte[] publicKeyToken)
    {
        _name = name;
        _publicKeyToken = BinaryPrimitives.ReadUInt64LittleEndian(publicKeyToken);
    }
}

//>>>> CORELIB <<<<
internal partial class CompilationContext
{
    public int CORELIB_ID => _deps[0];
    public VAssembly CORELIB => DependencyAt(0);

    //CORELIB_TYPES
    private static readonly ArrayView<string> NSPACE_SYSTEM = ["System"];

    public VType TYPE_OBJECT => CORELIB.FindType(NSPACE_SYSTEM, new("Object"));
    public VType TYPE_STRUCT => CORELIB.FindType(NSPACE_SYSTEM, new("Value"));
    public VType TYPE_VOID => CORELIB.FindType(NSPACE_SYSTEM, new("Void"));

    public VType TYPE_INT8 => CORELIB.FindType(NSPACE_SYSTEM, new("SByte"));
    public VType TYPE_UINT8 => CORELIB.FindType(NSPACE_SYSTEM, new("Byte"));
    public VType TYPE_INT16 => CORELIB.FindType(NSPACE_SYSTEM, new("Int16"));
    public VType TYPE_UINT16 => CORELIB.FindType(NSPACE_SYSTEM, new("UInt16"));
    public VType TYPE_INT32 => CORELIB.FindType(NSPACE_SYSTEM, new("Int32"));
    public VType TYPE_UINT32 => CORELIB.FindType(NSPACE_SYSTEM, new("UInt32"));
    public VType TYPE_INT64 => CORELIB.FindType(NSPACE_SYSTEM, new("Int64"));
    public VType TYPE_UINT64 => CORELIB.FindType(NSPACE_SYSTEM, new("UInt64"));

    public VType TYPE_FLOAT32 => CORELIB.FindType(NSPACE_SYSTEM, new("Single"));
    public VType TYPE_FLOAT64 => CORELIB.FindType(NSPACE_SYSTEM, new("Double"));

    public VType TYPE_INTPTR => CORELIB.FindType(NSPACE_SYSTEM, new("IntPtr"));
    public VType TYPE_UINTPTR => CORELIB.FindType(NSPACE_SYSTEM, new("UIntPtr"));

    public VType TYPE_CHAR => CORELIB.FindType(NSPACE_SYSTEM, new("Char"));
    public VType TYPE_BOOL => CORELIB.FindType(NSPACE_SYSTEM, new("Boolean"));
    public VType TYPE_STRING => CORELIB.FindType(NSPACE_SYSTEM, new("String"));
}