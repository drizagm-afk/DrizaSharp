using System.Text;
using DrzSharp.Compiler.Virtual;
using Mono.Cecil;

namespace DrzSharp.Compiler;

internal partial class CompilationContext
{
    internal int LoadDependency(string path)
    {
        AssemblyHash hash = new(path);

        if (TryGetLoadedAssembly(hash, out var vasm))
            return AddDependency(vasm);

        var asm = ReadAssembly(path);
        if (HasDependency(asm.DependencyName()))
            throw new Exception($"TRIED TO LOAD ASSEMBLY TWICE <{asm.Name.Name}>");

        return TryLoadAssembly(asm, hash);
    }

    //>>>> ASSEMBLY LOADING <<<<
    private readonly static ReaderParameters _readParams = new()
    {
        ReadSymbols = false,
        ReadingMode = ReadingMode.Deferred,
        InMemory = true,
        ReadWrite = false
    };
    private static AssemblyDefinition ReadAssembly(string path)
    => AssemblyDefinition.ReadAssembly(path, _readParams);
    private int TryLoadAssembly(AssemblyDefinition definition, AssemblyHash hash)
    {
        ResolveDependencies(definition);

        //ADD DEPENDENCY
        var vasm = AddAssembly(definition, hash);
        var depId = AddDependency(vasm);

        //BIND ASSEMBLY
        try { VirtualLoader.Load(this, vasm); }
        catch
        {
            RemoveLastAssemblyAndDependency();
            throw;
        }

        return depId;
    }
    private void ResolveDependencies(AssemblyDefinition definition)
    {
        var deps = definition.MainModule.AssemblyReferences;

        StringBuilder builder = new();
        foreach (var dep in deps)
        {
            if (!HasDependency(dep.DependencyName()))
            {
                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(dep.Name);
            }
        }

        if (builder.Length > 0)
            throw new Exception($"TRIED TO LOAD ASSEMBLY <{definition.Name.Name}> BEFORE LOADING ITS DEPENDENCIES: {builder}");
    }
}

//>>>> VIRTUAL LOADER (VASM) <<<<
internal static partial class VirtualLoader
{
    internal static void Load(CompilationContext ctx, VAssembly vasm)
    {
        VirtualContext vctx = new(ctx, vasm);

        void onLoadFailure(string phase, Exception e)
        => throw new($"[FAILED TO {phase} <{vctx.Definition.Name.Name}>] {e.Message}", e);

        //BIND ASSEMBLY
        try { BindAssembly(vctx); }
        catch (Exception e)
        {
            onLoadFailure("BIND ASSEMBLY", e);
        }

        //BIND ASSEMBLY DATA
        try { BindAssemblyData(vctx); }
        catch (Exception e)
        {
            onLoadFailure("BIND ASSEMBLY DATA", e);
        }
    }
    private readonly struct VirtualContext(CompilationContext ctx, VAssembly vasm)
    {
        public CompilationContext Ctx => ctx;

        public VAssembly Asm => vasm;
        public AssemblyDefinition Definition => Asm.Definition;
    }
}